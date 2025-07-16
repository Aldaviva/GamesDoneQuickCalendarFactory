namespace GamesDoneQuickCalendarFactory.Data;

public class ValueHolderStruct<T>(T? initialValue = null) where T: struct {

    public T? value { get; set; } = initialValue;

}

public class ValueHolderRef<T>(T? initialValue = null) where T: class {

    public T? value { get; set; } = initialValue;

}