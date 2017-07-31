namespace Cllearworks.COH.Repository
{
    public class BaseRepository
    {
        public COHEntities Context { get; set; }

        public BaseRepository()
        {
            Context = new COHEntities();
        }
    }
}
