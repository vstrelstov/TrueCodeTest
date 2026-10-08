# TrueCodeTest

Тестовое задание: микросервисы на .NET 8 с PostgreSQL.

## Структура решения

| Проект | Назначение |
|---|---|
| `TrueCodeTest.Shared` | Общая библиотека: сущности, `AppDbContext` с маппингом на таблицы `currency`, `user`, `user_favorite_currency`. |
| `TrueCodeTest.DbMigrator` | Микросервис миграций: применяет миграции EF Core к БД. |
| `TrueCodeTest.CurrencyUpdater` | Фоновый сервис: забирает курсы с сайта ЦБ РФ и наполняет таблицу `currency`. |

## Требования

- .NET SDK 8.0
- PostgreSQL 16 (в проекте используется контейнер `dev-postgres`, база `devdb`)

## База данных

Строка подключения задаётся ключом `ConnectionStrings:DefaultConnection`
в `appsettings.json` соответствующего проекта:

```
Host=localhost;Port=5432;Database=devdb;Username=postgres;Password=password
```

## Запуск

Применить миграции (создаёт таблицы `currency`, `user`, `user_favorite_currency`):

```bash
dotnet run --project TrueCodeTest.DbMigrator
```

Запустить фоновое обновление курсов:

```bash
dotnet run --project TrueCodeTest.CurrencyUpdater
```

## Пункт 2: фоновый сервис обновления курсов

Сервис обращается к `http://www.cbr.ru/scripts/XML_daily.asp`, разбирает ответ
и наполняет таблицу `currency`.

Особенности реализации:

- Лента отдаётся в кодировке `windows-1251`, поэтому ответ читается как байты
  и декодируется явно, а не силами `HttpClient`.
- Числа в ленте используют запятую как десятичный разделитель, а `VunitRate`
  для мелких валют приходит в экспоненциальной форме (`4,84195E-05`).
- В БД сохраняется курс **за одну единицу** валюты. Лента указывает `Value`
  для `Nominal` единиц, поэтому берётся `VunitRate`, а при его отсутствии
  `Value` делится на `Nominal`.
- Обновление идемпотентно: валюты сопоставляются по названию (`ix_currency_name`),
  новые добавляются, существующие обновляются только при изменении курса.
- Ошибка одного цикла не останавливает сервис: выполняются повторы
  с экспоненциальной паузой, после чего цикл завершается и ждёт следующего тика.

Настройки в секции `CurrencyUpdater` файла `appsettings.json`:

| Ключ | Значение по умолчанию | Описание |
|---|---|---|
| `CbrDailyUrl` | `https://www.cbr.ru/scripts/XML_daily.asp` | Адрес ленты ЦБ РФ |
| `Interval` | `01:00:00` | Пауза между циклами обновления |
| `RunOnStartup` | `true` | Выполнять обновление сразу при старте |
| `HttpTimeout` | `00:00:30` | Таймаут HTTP-запроса |
| `MaxAttempts` | `3` | Число попыток на один цикл |
| `RetryBaseDelay` | `00:00:02` | Базовая пауза между попытками |

## Миграции

Миграции лежат в `TrueCodeTest.DbMigrator/Migrations`, а контекст — в
`TrueCodeTest.Shared`, поэтому сборка миграций указывается явно через
`MigrationsAssembly`.

Создать новую миграцию:

```bash
dotnet tool restore
dotnet ef migrations add <Name> \
  --project TrueCodeTest.DbMigrator \
  --output-dir Migrations
```

## Статус по пунктам задания

- [x] 1. Микросервис миграции БД
- [x] 2. Фоновый сервис обновления курсов
- [ ] 3. Микросервис пользователя (регистрация, логин, логаут)
- [ ] 4. Микросервис финансов (курсы по пользователю)
- [ ] 5. Авторизация JWT
- [ ] 6. API Gateway
- [ ] 7. Unit-тесты
