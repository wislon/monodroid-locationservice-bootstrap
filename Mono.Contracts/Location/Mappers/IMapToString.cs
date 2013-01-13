namespace Mono.Contracts.Location.Mappers
{
    public interface IMapToString<T>
    {
        string MapToString(T data);
    }

}