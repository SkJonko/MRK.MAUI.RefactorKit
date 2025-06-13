# Error Codes

| Code     | Title                        | Severity | Description                                                                                                 |
|----------|------------------------------|----------|-------------------------------------------------------------------------------------------------------------|
| [MRK0001](https://github.com/SkJonko/MRK.MAUI.RefactorKit/tree/main/docs/rules/MRK0001.md)  | Property uses OnPropertyChanged/SetProperty in setter | Error    | Flags properties that call `OnPropertyChanged` or `SetProperty` in their setter. Use `[NotifyPropertyChangedFor(nameof(xxxxx))]` - `[ObservableProperty]` instead for MVVM best practices. |
