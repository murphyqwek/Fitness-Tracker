# Loki, Prometheus и Grafana

Отдельный стек для сервера мониторинга с Docker Compose v2. Порты доступны только на localhost: Grafana — `3000`, Loki — `3100`, Prometheus — `9090`.

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
docker compose run --rm --no-deps --entrypoint /bin/promtool prometheus check config /etc/prometheus/prometheus.yml
docker compose up -d
```

## Доступ через SSH

На своём компьютере выполните, подставив пользователя и IP сервера:

```bash
ssh -N -o ExitOnForwardFailure=yes -L 127.0.0.1:3000:127.0.0.1:3000 USER@MONITOR_PUBLIC_IP
```

Откройте [Grafana](http://127.0.0.1:3000) и войдите с данными из `.env`. Источники Loki и Prometheus подключены автоматически; Loki остаётся источником по умолчанию. Логи появятся после их отправки, метрики — после подключения эндпоинта приложения.

## Конфиги

| Файл | Что настраивает |
| --- | --- |
| `compose.yaml` | Контейнеры, сеть, порты и volumes |
| `loki/loki.yaml` | Хранение и удаление логов |
| `prometheus/prometheus.yml` | Адреса и интервалы сбора метрик |
| `grafana/grafana.ini` | Настройки Grafana |
| `grafana/provisioning/datasources/loki.yaml` | Подключение Grafana к Loki |
| `grafana/provisioning/datasources/prometheus.yaml` | Подключение Grafana к Prometheus |

Конфиги подключены к контейнерам в режиме чтения. После их изменения выполните `docker compose restart loki prometheus grafana`. После изменения Compose или `.env` — `docker compose up -d`.

## Проверка

На сервере:

```bash
docker compose ps
docker compose logs --tail=50 loki prometheus grafana
curl --fail http://127.0.0.1:3100/ready
curl --fail http://127.0.0.1:9090/-/ready
```

## Метрики приложения через WireGuard

Prometheus опрашивает `http://10.50.0.1:9091/metrics` каждые 15 секунд с таймаутом 5 секунд. Если WireGuard-адрес сервера приложения другой, измените `targets` в `prometheus/prometheus.yml`.

На сервере приложения нужно отдельно подключить сервер метрик на порту `9091` и опубликовать его только на WireGuard-адресе. Правила фильтрации с учётом Docker должны разрешать доступ только через WireGuard от сервера мониторинга. Проверьте доступность эндпоинта из контейнерной сети мониторинга; доступность с самого хоста не гарантирует доступ из контейнера. До подключения эндпоинта цель `fitness-api` будет иметь статус DOWN.

В Grafana откройте Explore, выберите Prometheus и выполните `up{job="fitness-api"}`. Значение `1` означает успешный последний опрос, `0` — ошибку опроса.

Для интерфейса Prometheus откройте отдельный SSH-туннель:

```bash
ssh -N -o ExitOnForwardFailure=yes -L 127.0.0.1:9090:127.0.0.1:9090 USER@MONITOR_PUBLIC_IP
```

Откройте [Prometheus](http://127.0.0.1:9090) и проверьте цель `fitness-api` на странице Targets. Публиковать порт `9090` на WireGuard-адресе не требуется: Prometheus сам инициирует запросы к приложению, а Grafana обращается к нему внутри Docker-сети.

## Alloy через WireGuard

Когда потребуется приём логов с другого сервера, сначала поднимите WireGuard с адресом `10.50.0.2`, затем:

```bash
docker compose -f compose.yaml -f compose.wireguard.yaml up -d
```

Loki дополнительно станет доступен на `10.50.0.2:3100`. Адрес отправки для Alloy — `http://10.50.0.2:3100/loki/api/v1/push`. Для последующих команд Compose используйте те же два `-f`.

Логи хранятся семь дней, метрики — 15 дней, данные — в постоянных volumes. Команда `docker compose down -v` удаляет эти данные.
