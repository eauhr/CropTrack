namespace CropTrack.Repositories
{
    public interface IDb<T, K>
    {
        void Create(T item);
        T Read(K key, bool useNavigationalProperties = false, bool isReadOnly = false);
        List<T> ReadAll(bool useNavigationalProperties = false, bool isReadOnly = false);
        void Update(T item, bool useNavigationalProperties = false);
        void Delete(K key);
    }
}
