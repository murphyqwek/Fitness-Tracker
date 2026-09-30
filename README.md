# Fitness-Tracker

![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18-316192?style=flat&logo=postgresql&logoColor=white)
![Redis](https://img.shields.io/badge/Redis_Stack-DC382D?style=flat&logo=redis&logoColor=white)
![Apache Kafka](https://img.shields.io/badge/Apache_Kafka-231F20?style=flat&logo=apachekafka&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white)
![Angular](https://img.shields.io/badge/Frontend-Angular-DD0031?style=flat&logo=angular&logoColor=white)

**Fitness-Tracker** — веб-приложение для записи, хранения и анализа тренировок.

Основной API отвечает за работу с пользователями, упражнениями и тренировками.  
События о завершённых тренировках передаются через **Apache Kafka** в отдельный сервис аналитики, который рассчитывает тренировочный объём и недельные рекорды.

**Репозиторий фронтенда:** [Fitness-Tracker Frontend](https://github.com/murphyqwek/fintess-tracker-client)

**Сайт:** [Fitracker](https://fitracker.online)

---

## Примеры интерфейса

<div align="center">
  <img src="docs/img/dashboard.png" alt="Главная страница" width="80%">
  <p><em>Главный дашборд и история тренировок</em></p>

  <img src="docs/img/exercises.png" alt="Каталог упражнений" width="80%">
  <p><em>Поиск упражнений и фильтрация по группам мышц</em></p>
</div>

---

## Основные возможности

- **Каталог упражнений:** быстрый поиск упражнений по названию и целевым мышечным группам
- **Управление тренировками:** создание, сохранение и просмотр истории тренировочных сессий
- **Аналитика:** расчёт тренировочного объёма за месяц и недельного рекорда на основе e1RM
- **Асинхронная обработка событий:** передача событий о завершённых тренировках через **Apache Kafka**
- **Надёжная публикация событий:** сохранение сообщений через **Transactional Outbox** перед отправкой в Kafka
- **Кэширование:** использование **Redis Stack Server**
- **Безопасность:** JWT-аутентификация с RSA-подписью и Refresh-токенами
- **Контейнеризация:** запуск приложения и инфраструктуры через Docker Compose

---
## Стек технологий

- **Backend:** ASP.NET Core, Entity Framework Core
- **База данных:** PostgreSQL
- **Кэширование:** Redis Stack Server
- **Message Broker:** Apache Kafka
- **Асинхронная обработка:** Kafka Producer / Consumer, Transactional Outbox
- **Аутентификация:** JWT, RSA
- **Контейнеризация:** Docker, Docker Compose
- **Reverse Proxy:** Nginx
- **CI/CD:** GitHub Actions
- **Frontend:** Angular, TypeScript, Tailwind CSS

---

## Быстрый старт (Docker Compose)

### 1. Клонирование репозитория

```bash
git clone https://github.com/murphyqwek/fitness-tracker.git
cd fitness-tracker
```

### 2. Настройка переменных окружения (`.env`)

Создайте файл `.env` в корневой директории проекта рядом с `docker-compose.yml`:

```env
# Docker images
IMAGE_NAME=fitness-tracker-api:latest
ANALYTICS_IMAGE_NAME=fitness-tracker-analytics:latest

# PostgreSQL
DB_NAME=fitness_tracker_db
DB_USER_APP=postgres
DB_PASSWORD_APP=your_strong_password
DB_OUT_PORT=5432
DB_MAX_POOL_SIZE=4

# Redis
REDIS_PASSWORD=your_redis_password

# JWT
JWT_PUBLIC_KEY_HOST_PATH=/path/to/secrets/jwt_public.pem
JWT_PRIVATE_KEY_HOST_PATH=/path/to/secrets/jwt_private.pem
```

Kafka запускается как отдельный контейнер и доступна сервисам внутри Docker-сети по адресу:

```text
kafka:29092
```

Для подключения с хоста используется:

```text
localhost:9092
```

### 3. Сборка Docker-образов

Основной API:

```bash
docker build -t fitness-tracker-api:latest .
```

Сервис аналитики:

```bash
docker build \
  -f Fintess-Tracker-Analytics/Dockerfile \
  -t fitness-tracker-analytics:latest \
  .
```

### 4. Запуск сервисов

```bash
docker compose up -d
```

Docker Compose поднимет:

- Fitness Tracker API
- Analytics Service
- PostgreSQL
- Redis Stack
- Apache Kafka

### 5. Проверка Kafka

Посмотреть список топиков:

```bash
docker exec kafka \
  /opt/kafka/bin/kafka-topics.sh \
  --bootstrap-server localhost:29092 \
  --list
```

Основной топик приложения:

```text
workout.completed
```

Если автоматическое создание топиков отключено, его можно создать вручную:

```bash
docker exec kafka \
  /opt/kafka/bin/kafka-topics.sh \
  --bootstrap-server localhost:29092 \
  --create \
  --topic workout.completed \
  --partitions 1 \
  --replication-factor 1
```

### 6. Доступ к сервисам

После запуска будут доступны:

- 🌐 **Backend API:** `http://localhost:8080`
- 📊 **Analytics API:** `http://localhost:4545`
- 🐘 **PostgreSQL:** `localhost:${DB_OUT_PORT}`
- 🔴 **Redis Stack:** `localhost:6379`
- 📨 **Apache Kafka:** `localhost:9092`

Остановка контейнеров:

```bash
docker compose down
```

---

##  Наполнение базы данных (Сидинг)

В папке **`Database Scripts`** подготовлены готовые SQL-скрипты для первичного наполнения базы данных начальными справочниками:

1. **Скрипт групп мышц** - добавляет 15 основных анатомических мышечных групп
2. **Скрипт упражнений** - наполняет базу каталогом из 100 базовых и изолирующих упражнений с готовой привязкой к соответствующим группам мышц

> *Выполните эти скрипты по очереди после первого применения миграций через любой удобный инструмент (pgAdmin, DBeaver или консольный `psql`).*

---

## TODO

- [ ] **Расширенная аналитика:** построение графиков прогресса рабочих весов (1RM) и диаграмм распределения нагрузки
- [ ] Пользовательские шаблоны тренировок
- [ ] Подробное описание выполнения упражнений
- [ ] Видео-примеры выполнения упражнений
- [ ] Добавление встроенного таймера отдыха между подходами
- [ ] Интеграция с Telegram-ботом для напоминаний и быстрых заметок
- [ ] Интеграция **LLM** для преобразования неструктурированных пользовательских заметок о тренировках к программному стандарту

---

## Лицензия

Этот проект распространяется под лицензией [MIT](LICENSE).
