# Alloy на сервере приложения

Собирает stdout/stderr контейнеров `app-container` и `analytcs-container` и отправляет в Loki через WireGuard. Требуется Linux с Docker Compose v2.

## Подготовка Loki

На сервере мониторинга из каталога с его Compose:

```bash
docker compose -f compose.yaml -f compose.wireguard.yaml up -d
```

WireGuard должен быть поднят до запуска Loki, включая перезагрузку сервера. При дальнейших командах мониторинга используйте оба `-f`.

## Запуск Alloy

Перенесите этот каталог на сервер приложения в `/opt/fitness-alloy`:

```bash
cd /opt/fitness-alloy
curl --fail http://10.50.0.2:3100/ready
docker compose config --quiet
docker compose run --rm --no-deps alloy validate /etc/alloy/config.alloy
docker compose up -d
docker compose logs --tail=50 alloy
```

Alloy использует сеть хоста для доступа к WireGuard. Его интерфейс доступен только на `127.0.0.1:12345`; публичные порты не открываются.

## Конфиг

`config.alloy` содержит цепочку:

```text
discovery.docker → discovery.relabel → loki.source.docker → loki.write
найти контейнеры → добавить метки → прочитать логи → отправить в Loki
```

Имена контейнеров заданы в `filter` и правилах `discovery.relabel`. Адрес Loki — в `loki.write`, окружение и имя сервера — в `labels`. После изменения файла выполните `docker compose restart alloy`.

Docker socket нужен для доступа к Docker API. Монтирование socket с `read_only` не ограничивает разрешённые операции самого API.

Volume `alloy_data` сохраняет позиции чтения между перезапусками. Это не гарантированная дисковая очередь доставки: при сбоях возможна потеря записей.

## Посмотреть логи

В Grafana откройте Explore, выберите Loki и период «последние 15 минут»:

```logql
{service_name=~"fitness-api|fitness-analytics"}
```

Если записей нет, сначала проверьте `docker logs --tail=20 app-container` на сервере приложения. Alloy передаёт уже существующий вывод; подключение Serilog будет следующим шагом.

В основном `docker-compose.yml` приложения настроена ротация: три файла по 10 МБ на контейнер. Перенесите обновлённый файл и примените через `docker compose up -d` из каталога приложения — контейнеры с изменёнными настройками будут пересозданы. Чтение Alloy не удаляет локальные логи; их удаляет ротация Docker.
