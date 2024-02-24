namespace LivePlaylistsClone.Channels
{
    public interface IChannel
    {
        string Name { get; }
        string StreamUrl { get; }
    }
}
