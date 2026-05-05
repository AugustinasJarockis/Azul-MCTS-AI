using static TorchSharp.torch;

namespace AzulAIAndGameState.Players.MCTS_NN.Networks
{
    public interface INetwork
    {
        public Device Device { get; }
        public Tensor Call(Tensor x);
        public void TrainWithLoss(Tensor loss);

        public void Save(string path);
        public void Load(string path);
    }
}
