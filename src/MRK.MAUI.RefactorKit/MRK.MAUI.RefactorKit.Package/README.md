# Error Codes

| Code     | Title                        | Severity | Description                                                                                                 |
|----------|------------------------------|----------|-------------------------------------------------------------------------------------------------------------|
| MRK0001  | Property uses OnPropertyChanged/SetProperty in setter | Error    | Flags properties that call `OnPropertyChanged` or `SetProperty` in their setter. Use `[ObservableProperty]` instead for MVVM best practices. |


### Examples

<details>
    <summary>MRK0001</summary>

### Before:

```
private string _name;
public string Name
{
    get => _name;
    set
    {
        _name = value;
        OnPropertyChanged(nameof(Name));
    }
}
```

### After:

```
[ObservableProperty]
public partial string Name { get; set; }
```

---

### Before:

```
private string _test;
public string Test
{
    get => _test;
    set
    {
        _test = value;
        OnPropertyChanged("CanExecuteCommand");
    }
}
```

### After:

```
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(CanExecuteCommand))]
public partial string Test { get; set; }
```

---

### Before:

```
private string _test1;
public string Test1
{
    get => _test1;
    set
    {
        _test1 = value;
        OnPropertyChanged(nameof(CanExecuteCommand));
    }
}
```

### After:

```
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(CanExecuteCommand))]
public partial string Test1 { get; set; }
```

---

### Before:

```
private bool _canExecuteCommand = false;
public bool CanExecuteCommand
{
    get { return _canExecuteCommand; }
    set { SetProperty(ref _canExecuteCommand, value); }
}
```

### After:

```
[ObservableProperty]
public partial bool CanExecuteCommand { get; set; } = false;
```

</details>