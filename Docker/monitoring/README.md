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

Откройте [Grafana](http://127.0.0.1:3000) и войдите с данными из `.env`. Источники Loki и Prometheus подключены автоматически; Loki остаётся источником по умолчанию. Дашборд **Fitness Tracker → Fitness API** также подключается автоматически. Логи появятся после их отправки, метрики — после подключения эндпоинта приложения.

## Конфиги

| Файл | Что настраивает |
| --- | --- |
| `compose.yaml` | Контейнеры, сеть, порты и volumes |
| `loki/loki.yaml` | Хранение и удаление логов |
| `prometheus/prometheus.yml` | Адреса и интервалы сбора метрик |
| `grafana/grafana.ini` | Настройки Grafana |
| `grafana/provisioning/datasources/loki.yaml` | Подключение Grafana к Loki |
| `grafana/provisioning/datasources/prometheus.yaml` | Подключение Grafana к Prometheus |
| `grafana/provisioning/dashboards/fitness.yaml` | Загрузка дашборда в папку Fitness Tracker |
| `grafana/dashboards/fitness-api.json` | Шесть панелей метрик Fitness API |

Конфиги подключены к контейнерам в режиме чтения. После их изменения выполните `docker compose restart loki prometheus grafana`. После изменения Compose или `.env` — `docker compose up -d`.

## Дашборд Fitness API

В репозитории уже подготовлен [JSON дашборда](grafana/dashboards/fitness-api.json). Панели используют источник с UID `prometheus`, заданный в provisioning источников:

| Панель | Что показывает |
| --- | --- |
| Доступность API | `UP` / `DOWN` по результату последнего опроса `/metrics` |
| Новые тренировки · 1 час | Прирост счётчика за скользящий час |
| Результаты входа · 1 час | Успешные и неуспешные проверки учётных данных |
| HTTP-запросы в секунду | Частоту запросов, усреднённую за 5 минут |
| p95 получения упражнений | Время поиска и получения по ID за окно 5 минут |
| Очередь Outbox | Изменение числа необработанных сообщений |

### Подключить к существующему стеку

Перенесите на сервер мониторинга обновлённый `compose.yaml` и оба каталога `grafana/dashboards/` и `grafana/provisioning/dashboards/`, сохранив структуру. Выполните из каталога мониторинга:

```bash
docker compose config --quiet
docker compose up -d grafana
docker compose logs --tail=50 grafana
```

Если Loki опубликован через WireGuard, используйте те же файлы Compose, с которыми запущен стек:

```bash
docker compose -f compose.yaml -f compose.wireguard.yaml config --quiet
docker compose -f compose.yaml -f compose.wireguard.yaml up -d grafana
```

После пересоздания контейнера откройте [Fitness API](http://127.0.0.1:3000/d/fitness-api) через SSH-туннель. Дашборд расположен в папке **Fitness Tracker**, период по умолчанию — 30 минут, обновление — 15 секунд.

Панели загружаются через [файловый provisioning Grafana](https://grafana.com/docs/grafana/latest/administration/provisioning/#dashboards). Изменения JSON подхватываются примерно за 30 секунд; сохранение правок из интерфейса отключено, исходник хранится в Git. Можно сохранить отдельную копию дашборда для экспериментов.

### Импортировать вручную

Чтобы посмотреть дашборд без изменения серверного Compose, в Grafana откройте **Dashboards → Import**, загрузите `grafana/dashboards/fitness-api.json` и подтвердите импорт. В этой Grafana должен существовать источник с UID `prometheus`. Используйте один способ подключения для UID `fitness-api`: при последующем provisioning файловая версия заменит ручную.

### Проверить данные

Сначала выполните в Explore → Prometheus `up{job="fitness-api"}`. Для рабочего опроса должно быть `1`. Затем несколько минут выполняйте поиск, открывайте карточки упражнений, создайте тестовые тренировки и выполните вход с верным и неверным паролем. Подробный сценарий — в [инструкции по скриншотам](../../docs/screenshots.md).

`Нет данных` отличается от `DOWN`: первое означает отсутствие ряда, второе — неуспешный опрос существующей цели. `UP` проверяет доступность метрик, а не работоспособность всех функций API. `rate` и `increase` требуют нескольких опросов; без обращений к упражнениям p95 не определён. `increase` может быть дробным из-за экстраполяции. Ноль в Outbox при исправной доставке — ожидаемый результат; не заменяем отсутствие данных нулём.

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
