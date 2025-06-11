
public class DropdownOption<T>
{
  public string DisplayName;
  public T Value;

  public DropdownOption(string displayName, T value)
  {
    DisplayName = displayName;
    Value = value;
  }
}

