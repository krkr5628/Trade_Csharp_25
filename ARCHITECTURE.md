# System Architecture Document

## 1. System Overview

This project is an automated stock trading application built with C# and Windows Forms. It is designed to execute trades based on user-defined strategies and conditions, using multiple brokerage APIs. It primarily supports the Kiwoom Open API and now includes full support for the Korea Investment & Securities (KIS) API. The application features a graphical user interface (GUI) for monitoring, configuration, and viewing logs. It also includes integration with Telegram for notifications and remote control, and can receive trade signals via TradingView Webhooks.

## 2. System Architecture

The application follows a **monolithic architecture**. The core logic, UI, and API interactions are tightly coupled within a single executable. The primary design is UI-driven, with most operations initiated by user actions or timers within the main form.

A significant architectural characteristic is the use of a **"God Class" (`Form1.cs`)**, which acts as the central hub for almost all application logic. This class is responsible for UI event handling, trading strategy execution, API communication, state management, and more.

Other components, such as settings and utility classes, are implemented as static classes, serving as global containers for state and helper functions. This leads to tight coupling between components and makes the system difficult to maintain and test.

A high-level view of the component interaction is as follows:

-   **`Program.cs`**: Starts the application by launching the main form (`Form1`).
-   **`Form1` (`Trade_Auto`)**: The central component.
    -   It initializes and manages other forms (`Form2` through `Form5`).
    -   It uses `utility.cs` to access global settings.
    -   It uses `utility_KIS.cs` for KIS API calls.
    -   It handles all events from the Kiwoom API.
    -   It includes a listener for TradingView webhooks to trigger trades.
-   **`Form2` (`Setting`)**: Reads from and writes to `utility.cs` to manage settings. It is opened by `Form1`.
-   **Other Forms (`Form3`, `Form4`, `Form5`)**: Display data (logs, transactions) read from files or passed from `Form1`.
-   **`utility.cs`**: A global, static state store for all settings, read from `setting.txt`.

## 3. Core Components and Functionality

### 3.1. `Program.cs`

-   **Purpose**: The main entry point of the application.
-   **Key Functionality**:
    -   `Main()`: Initializes the application's visual styles and runs the main form, `Trade_Auto` (defined in `Form1.cs`).

### 3.2. `Form1.cs` (`Trade_Auto`)

-   **Purpose**: The main application window and the central logic controller. It is a very large and complex class with numerous responsibilities.
-   **Key Functionality**:
    -   **UI Event Handling**: Manages all user interactions on the main form, such as button clicks for logging in, starting/stopping trading, and opening other forms.
    -   **Kiwoom API Interaction**:
        -   `onEventConnect`: Handles login success/failure events.
        -   `onReceiveTrData`: A large `switch` statement that processes all transactional data received from Kiwoom, such as account balance, stock information, and order confirmations.
        -   `onReceiveRealData`: Processes real-time market data for subscribed stocks.
        -   `onReceiveConditionVer`, `onReceiveTrCondition`, `onReceiveRealCondition`: Manage the retrieval and real-time monitoring of user-defined trading conditions (formulas).
        -   `onReceiveChejanData`: Processes real-time order execution data.
    -   **Trading Logic**:
        -   `buy_check()`: Contains the core logic to determine if a buy order should be placed, checking against numerous conditions (time, balance, max trades, index status, etc.).
        -   `sell_check_price()`, `sell_check_condition()`: Contain the logic to trigger sell orders based on profit/loss targets, trailing stops, or sell-side conditions.
        -   `sell_order()`: Constructs and sends the sell order via the Kiwoom API.
    -   **State Management**:
        -   Manages several `DataTable` objects (`dtCondStock`, `dtCondStock_hold`, `dtCondStock_Transaction`) to hold the real-time state of watched stocks, portfolio holdings, and transaction history.
    -   **Timers**: Uses multiple timers (`timer1`, `timer2`, `timer3`) to periodically update the UI (clock), check for trading conditions, and process queued data.
    -   **Telegram Integration**:
        -   `telegram_message()`: Sends formatted messages to the user's Telegram.
        -   `Telegram_Receive()`: Polls the Telegram API for incoming commands and processes them (e.g., `/START`, `/STOP`).
    -   **External Data Fetching**:
        -   `US_INDEX()`: Fetches US market index data from Yahoo Finance.
        -   `KOR_INDEX()`: Fetches Korean futures index data via the Kiwoom API.
        -   `KOR_FOREIGN_COMMUNICATION()`: Listens on a named pipe for external data (e.g., foreign commodity data).

### 3.3. `Form2.cs` (`Setting`)

-   **Purpose**: Provides a detailed UI for configuring all application settings.
-   **Key Functionality**:
    -   **Load/Save Settings**: `setting_save()` and `match()` handle the saving and loading of settings to/from a text file. The mechanism is fragile, relying on a fixed line order.
    -   **Input Validation**: Contains dozens of event handlers (`Leave` events) to validate user input for every setting, ensuring data is in the correct format and range.
    -   **UI Controls**: Manages a large number of UI controls that map directly to the settings variables in `utility.cs`.
    -   **Apply Settings**: An "Apply" function allows the user to hot-reload the settings into the main application.

### 3.4. `Form3.cs` (`Transaction`)

-   **Purpose**: Displays historical trade data.
-   **Key Functionality**:
    -   **Log Discovery**: Reads a hardcoded directory (`C:\Auto_Trade_Kiwoom\Log_Trade`) to find and list available trade logs by date.
    -   **Log Parsing**: When a log is selected, it uses a Regex pattern to parse the file line by line and extract individual trade details.
    -   **Data Display**: Populates two grids: one with a raw list of trades and another with an aggregated summary of profit/loss per stock.

### 3.5. `Form4.cs` (`Log`)

-   **Purpose**: A simple viewer for the application's full, detailed logs.
-   **Key Functionality**:
    -   **Log Discovery**: Reads a hardcoded directory (`C:\Auto_Trade_Kiwoom\Log`) to find and list available log files.
    -   **Display**: Loads the entire content of a selected log file into a `RichTextBox`.

### 3.6. `Form5.cs` (`Update`)

-   **Purpose**: Displays static information (agreements, update notes) and handles a custom authentication mechanism.
-   **Key Functionality**:
    -   **Display Content**: Reads and displays content from hardcoded `Agreement.txt` and `Update.txt` files.
    -   **Authentication**: Contains UI and logic to send an authentication code to a server. This feature is currently **non-functional** as it uses a placeholder URL.

### 3.7. `utility.cs`

-   **Purpose**: Acts as a global, static container for all application settings.
-   **Key Functionality**:
    -   **Global State**: Contains dozens of `public static` fields that hold the application's configuration.
    -   **Settings Loading**: `auto_load()` reads the `setting.txt` file line by line, parsing each value and assigning it to the corresponding static field. This process is highly brittle and lacks error handling.

### 3.8. `utility_KIS.cs`

-   **Purpose**: Encapsulates the logic for interacting with the Korea Investment & Securities (KIS) Open API.
-   **Key Functionality**:
    -   **API Wrappers**: Contains methods for key KIS API endpoints:
        -   `KIS_WebSocket()`: Gets a WebSocket approval key.
        -   `KIS_Access()`: Gets an access token.
        -   `KIS_Order()`: Places buy/sell orders.
        -   `KIS_Depositt()`: Checks account balance.
    -   **Hardcoded Endpoints**: The URLs for the KIS API (both mock and production) are hardcoded, requiring manual code changes to switch environments.

## 4. Data Management

-   **In-Memory State**: The application's real-time trading state (e.g., list of monitored stocks, portfolio) is held in-memory within `DataTable` objects in the `Form1` class.
-   **Configuration**: All user settings are stored in a plain text file (`setting.txt`). The file is read at startup and parsed line by line. This is not a robust method for managing configuration.
-   **Logging**: The application generates two types of logs: a detailed full log (`_full.txt`) and a trade-specific log (`_trade.txt`). These are stored in hardcoded directories.

## 5. External Dependencies

-   **Kiwoom Open API**: The primary API used for all trading, market data, and account information.
-   **Korea Investment & Securities (KIS) Open API**: A secondary API used for trading.
-   **TradingView Webhooks**: Used to receive trade signals from TradingView alerts.
-   **Telegram API**: Used for sending notifications and receiving commands via HTTP requests.
-   **Yahoo Finance**: Used via unofficial HTTP requests to fetch US market index data.
-   **Newtonsoft.Json**: Used for serializing and deserializing JSON data for API communication.
