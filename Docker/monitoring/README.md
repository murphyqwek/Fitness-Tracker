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

В API сервер метрик работает отдельно на порту `9091`; на публичном порту `8080` маршрут `/metrics` не добавляется. HTTP-метрики собирает `UseHttpMetrics()`, бизнес-метрики — singleton `PrometheusFitnessMetrics` из Infrastructure через интерфейс `IFitnessMetrics` из Application.

На сервере приложения после запуска WireGuard примените из корня проекта:

```bash
docker compose -f docker-compose.yml -f docker-compose.metrics.yml config --quiet
docker compose -f docker-compose.yml -f docker-compose.metrics.yml up -d
```

Дополнительный файл публикует `9091` только на `10.50.0.1`. При другом адресе задайте `APP_WIREGUARD_IP` в `.env` приложения и измените `targets` Prometheus. Для последующих команд приложения используйте оба `-f`. Перед запуском разместите на сервере новый `docker-compose.metrics.yml` и обновлённый образ API.

Правила фильтрации с учётом Docker должны разрешать доступ только через WireGuard от сервера мониторинга. Проверьте доступность эндпоинта из контейнерной сети мониторинга; доступность с самого хоста не гарантирует доступ из контейнера. До подключения эндпоинта цель `fitness-api` будет иметь статус DOWN.

В Grafana откройте Explore, выберите Prometheus и выполните `up{job="fitness-api"}`. Значение `1` означает успешный последний опрос, `0` — ошибку опроса.

Для интерфейса Prometheus откройте отдельный SSH-туннель:

```bash
ssh -N -o ExitOnForwardFailure=yes -L 127.0.0.1:9090:127.0.0.1:9090 USER@MONITOR_PUBLIC_IP
```

Откройте [Prometheus](http://127.0.0.1:9090) и проверьте цель `fitness-api` на странице Targets. Публиковать порт `9090` на WireGuard-адресе не требуется: Prometheus сам инициирует запросы к приложению, а Grafana обращается к нему внутри Docker-сети.

### Метрики API

| Метрика | Что измеряет |
| --- | --- |
| `http_requests_received_total`, `http_request_duration_seconds`, `http_requests_in_progress` | Встроенные HTTP-метрики: запросы, длительность и текущая нагрузка |
| `fitness_workouts_created_total` | Новые тренировки после сохранения в БД; обновления не учитываются |
| `fitness_users_registered_total` | Новые пользователи после сохранения в БД, до обновления кеша |
| `fitness_login_attempts_total{result="success\|failure"}` | Завершённые проверки логина и пароля; исключения и отказы валидации до обработчика не учитываются |
| `fitness_outbox_pending_messages` | Последнее измеренное количество всех сообщений с `ProcessedAt IS NULL`, включая сообщения за пределами батча из 100 |
| `fitness_exercise_retrieval_duration_seconds{operation="search\|by_id"}` | Гистограмма времени обработки поиска списка или получения по ID, включая `FillCacheIfEmpty` и чтение результата |

Счётчики относятся к текущему процессу и сбрасываются при его перезапуске. Историю хранит Prometheus; это не точные итоговые количества записей в БД. Gauge Outbox обновляется перед обработкой каждого батча (обычный интервал — 2 секунды); до первого успешного чтения БД он не публикуется. Гистограмма упражнений фиксирует также завершение через исключение или отмену, без меток с текстом поиска и идентификаторами. Полный HTTP-запрос, включая валидацию и сериализацию ответа, измеряется HTTP-middleware.

Пример p95 времени получения упражнений по каждой операции:

```promql
histogram_quantile(0.95,
  sum by (le, operation) (
    rate(fitness_exercise_retrieval_duration_seconds_bucket[5m])
  )
)
```

## Alloy через WireGuard

Когда потребуется приём логов с другого сервера, сначала поднимите WireGuard с адресом `10.50.0.2`, затем:

```bash
docker compose -f compose.yaml -f compose.wireguard.yaml up -d
```

Loki дополнительно станет доступен на `10.50.0.2:3100`. Адрес отправки для Alloy — `http://10.50.0.2:3100/loki/api/v1/push`. Для последующих команд Compose используйте те же два `-f`.

Логи хранятся семь дней, метрики — 15 дней, данные — в постоянных volumes. Команда `docker compose down -v` удаляет эти данные.
