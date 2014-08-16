namespace EasyDOC.Model
{
    public interface IOrderedRelation<TParent, TChild> where TParent : IEntity where TChild : IEntity
    {
        TParent Parent { get; set; }
        TChild Child { get; set; }
        int Order { get; set; }
    }
}