# UtilityBills

Обрана тема: Комунальні платежі (варіант 12 з Додатку А).
Застосунок для обліку показань лічильників та розрахунку суми до сплати за комунальні послуги.

## Карта назв

| Роль у кістяку | У прикладі | Ваш варіант |
| :--- | :--- | :--- |
| Основний тип | `Card` | `Meter` |
| Різновид 1 | `OpenAnswerCard` | `SingleZoneMeter` |
| Різновид 2 | `ChoiceCard` | `DualZoneMeter` |
| Різновид 3 | `NumericCard` | `StandingChargeMeter` |
| Спільна дія | `Check(given)` | `CalculateBill(reading)` |
| Набір записів | `List<Card> deck` | `List<Meter> meters` |
| Сховище | `ICardStorage` | `IMeterStorage` |
| Порядок перебору | `IReviewStrategy` | `IBillingOrder` |
| Підсумок | `SessionSummary` | `MonthlySummary` |
| Різновид 4 (ЛР 8) | `TrueFalseCard` | `ThreeZoneMeter` |
