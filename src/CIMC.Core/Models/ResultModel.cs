namespace CIMC.Core.Models;

public class ResultModel
{
    public int Code { get; set; } = 200;

    public string Message { get; set; } = "成功";
}

public class ResultModel<T> : ResultModel
{
    public T? Data { get; set; }

    public int Count { get; set; }
}
