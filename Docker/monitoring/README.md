# Loki и Grafana

Отдельный стек для сервера мониторинга с Docker Compose v2. Порты доступны только на localhost: Grafana — `3000`, Loki — `3100`.

## Запуск

Перенесите этот каталог на сервер в `/opt/fitness-monitoring`:

```bash
cd /opt/fitness-monitoring
cp .env.example .env
chmod 600 .env
nano .env
```

Задайте `GRAFANA_ADMIN_PASSWORD` в `.env`, затем запустите:

```bash
docker compose config --quiet
docker compose up -d
```

## Доступ через SSH

На своём компьютере выполните, подставив пользователя и IP сервера:

```bash
ssh -N -o ExitOnForwardFailure=yes -L 127.0.0.1:3000:127.0.0.1:3000 USER@MONITOR_PUBLIC_IP
```

Откройте [Grafana](http://127.0.0.1:3000) и войдите с данными из `.env`. Источник Loki подключён автоматически; логи появятся после их отправки.

## Конфиги

| Файл | Что настраивает |
| --- | --- |
| `compose.yaml` | Контейнеры, сеть, порты и volumes |
| `loki/loki.yaml` | Хранение и удаление логов |
| `grafana/grafana.ini` | Настройки Grafana |
| `grafana/provisioning/datasources/loki.yaml` | Подключение Grafana к Loki |

Конфиги подключены к контейнерам в режиме чтения. После их изменения выполните `docker compose restart loki grafana`. После изменения Compose или `.env` — `docker compose up -d`.

## Проверка

На сервере:

```bash
docker compose ps
docker compose logs --tail=50 loki grafana
curl --fail http://127.0.0.1:3100/ready
```

## Alloy через WireGuard

Когда потребуется приём логов с другого сервера, сначала поднимите WireGuard с адресом `10.50.0.2`, затем:

```bash
docker compose -f compose.yaml -f compose.wireguard.yaml up -d
```

Loki дополнительно станет доступен на `10.50.0.2:3100`. Адрес отправки для Alloy — `http://10.50.0.2:3100/loki/api/v1/push`. Для последующих команд Compose используйте те же два `-f`.

Логи хранятся семь дней, данные — в постоянных volumes. Команда `docker compose down -v` удаляет эти данные.
