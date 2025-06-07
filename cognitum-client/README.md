# Cognitum Client

Клиентская часть проекта _Cognitum_ — мобильное приложение на Unity для тренировки когнитивных способностей.

## Требования

- Unity версии **не ниже 2022.3.35f1**
- Подключение к интернету для взаимодействия с сервером

## Структура проекта

```
Assets/
├── Fonts/                # Шрифты
├── Material/             # Материалы Unity
├── Music/                # Музыкальные файлы
├── Plugins/              # Сторонние библиотеки/SDK
├── Scenes/               # Все сцены проекта
├── ScriptableObjects/    # Игровые конфигурации и ScriptableObject-данные
├── Scripts/              # Скрипты проекта
├── Shader/               # Шейдеры
└── Sprites/              # Спрайты и изображения
```

## Конфигурация API

В файле `Assets/Scripts/API/APIConstants.cs` необходимо задать IP-адрес или домен сервера:

```csharp
public const string BaseUrl = "https://<ваш-домен-или-ip>/api";
```

### Доверенные сертификаты

Если используется HTTPS с доверенными сертификатами, удалить следующую строку из:

```
Assets/Scripts/AppScenes/StartApp/AutoLogin.cs
```

```csharp
ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
```

## Добавление новой мини-игры

1. **Создание сцены**  
   В `Assets/Scenes/Games/<категория_игры>` создайте папку новой игры. Скопируйте в неё шаблонную сцену:

   ```
   Assets/Scenes/Games/BuilderScene.unity
   ```

   Переименуйте сцену в название игры и добавьте её в Build Settings (меню `File → Build Settings → Add Open Scenes`).

2. **Создание игрового скрипта**  
   В `Assets/Scripts/Games/<категория_игры>` создайте класс, унаследованный от `AbstractGameBuilder`, и реализуйте его методы.  
   Реализуется **только поведение игры**, т.к. основной игровой менеджер (`GameManager`) управляет логикой уровней, таймером, очками и логгированием.

3. **Подключение скрипта на сцене**  
   На игровой сцене найдите объект `GameScene/Managers/GameManager`, повесьте на него созданный игровой скрипт.  
   Установите зависимости (если есть) и укажите ссылку на компонент в поле компонента `GameManager`.

4. **Размещение UI-элементов**  
   Все игровые объекты и UI-кнопки должны быть помещены внутрь `GameScene/Canvas/GameZone`.

5. **Добавление в список игр**  
   Откройте:

   ```
   Assets/ScriptableObjects/GameList/GamesData.asset
   ```

   Добавьте запись о новой игре.

   Подсказки к полям указаны в классе `GameData`.

6. **Добавление описания игры**  
   Создайте ScriptableObject `AboutGame` в:

   ```
   Assets/ScriptableObjects/AboutGame/
   ```

   Заполните все поля и поместите созданный объект в массив в `AllAboutGame.asset`.

7. **Добавление туториала**  
   Создайте `GameTutorial` в:

   ```
   Assets/ScriptableObjects/GameTutorials/
   ```

   Заполните туториал. Затем на сцене откройте:

   ```
   GameScene/Managers/UIManager
   ```

   И укажите созданный `GameTutorial` в поле `UIManager`.

## Изменение типа или логики игры

Для изменения типа или логики игры нужно создать новый класс GameManager, сохранив основную логику старого, и заменить старый GameManager на новый в игровой сцене.

## Репозиторий

- Обратная связь / багрепорты:  
  https://github.com/Calcifer08/Cognitum/issues
