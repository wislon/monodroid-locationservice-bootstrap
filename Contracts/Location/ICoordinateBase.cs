
namespace Contracts.Location
{
    /// <summary>
    /// Interface contract for establishing bare minimum requirement
    /// for capturing co-ordinates and associated data.
    /// </summary>
    public interface ICoordinateBase
    {
        string Id { get; set; }
    }
}