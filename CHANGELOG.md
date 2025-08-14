# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Nothing yet.

## [1.3.0] - 2025-08-13
### Added
- Full integration with Korea Investment & Securities (KIS) Open API for trading.
- Full integration with TradingView Webhooks for receiving trade signals.

## [1.2.0] - 2024-07-15
### Added
- Advanced index-based trading rules.
  - Link trading decisions to KOSPI and KOSDAQ futures indices.
  - Link trading decisions to US market indices (Dow Jones, S&P 500, NASDAQ).
  - Option to halt trading based on US market holidays.
- Named Pipe communication to receive foreign commodity index data from external programs.

## [1.1.0] - 2024-06-20
### Added
- Telegram integration for real-time notifications and remote control.
  - Receive summaries of trades, system status, and errors.
  - Send commands to the bot remotely (e.g., `/START`, `/STOP`, `/CLEAR`, `/REBOOT`).
- UI for viewing system, order, and stock-related logs separately.

## [1.0.0] - 2024-05-30
### Added
- Core automated trading engine using the Kiwoom Open API.
- Main user interface (UI) to monitor trading activity.
  - Real-time view of portfolio, open positions, and account balance.
  - UI forms for configuring trading strategies and settings.
- Support for multiple trading strategies based on Kiwoom's condition search system (OR, AND, Independent modes).
- Basic and advanced order settings:
  - Order types: Market and Limit orders.
  - Sell conditions: Profit target (%), loss cut (%), trailing stop.
- System for loading and saving trading settings from a file.
- Logging of all trading activities and system events to a file.

## [0.1.0] - 2024-04-10
### Added
- Initial C# Windows Forms project setup.
- Basic integration with the Kiwoom Open API.
- Successful login and retrieval of user account information.
- Proof-of-concept for receiving real-time stock data.
