# Error Codes

| Code     | Title                        | Severity | Description                                                                                                 |
|----------|------------------------------|----------|-------------------------------------------------------------------------------------------------------------|
| MRK0001  | Property uses OnPropertyChanged/SetProperty in setter | Error    | Flags properties that call `OnPropertyChanged` or `SetProperty` in their setter. Use `[ObservableProperty]` instead for MVVM best practices. |
