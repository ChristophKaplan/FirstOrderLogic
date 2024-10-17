namespace FirstOrderLogic;

public interface ILiteral : ISentence {
    IPredicate Pred { get; }
    Term[] Terms { get; }
}
