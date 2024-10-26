namespace Darklight.UnityExt.Behaviour
{
    /// <summary>
    /// An interface for a Visitor of type T.
    /// <br/> - A Visitor is a class that performs an operation on an element of type T.
    /// <br/> - The Visitor pattern is used to separate an algorithm from an object structure.
    /// <br/> - This implementation requires the element to be of type IVisitable<T>.
    /// </summary>
    public interface IVisitor<T>
    {
        void Visit(T element);
    }

    /// <summary>
    /// An interface for an element that can be visited by a Visitor of type T.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IVisitable<T>
    {
        void Accept(IVisitor<T> visitor);
    }
}