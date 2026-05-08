using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace AzulAIAndGameState.Players.MCTS_NN.Networks
{
    public class SmallLinearPolicyNetwork : Module, INetwork
    {
        public Adam optimizer;
        public Device Device { get; private set; }

        private Layer fc1;
        private Layer fc2;
        private Layer fc3;
        private Layer fc4;
        private Layer fc5;
        private Layer fc6;

        public SmallLinearPolicyNetwork(string modelPath = "") : base("SmallLinearPolicyNetwork")
        {
            if (cuda.is_available())
                Device = CUDA;
            else
                Device = CPU;

            fc1 = new Layer(301, 512);
            fc2 = new Layer(512, 512);
            fc3 = new Layer(512, 512);
            fc4 = new Layer(512, 512);
            fc5 = new Layer(512, 512);
            fc6 = new Layer(512, 180);

            RegisterComponents();

            if (modelPath != "")
            {
                load(modelPath).to(Device);
            }
            optimizer = optim.Adam(parameters(), lr: 0.001);
        }

        public Tensor Call(Tensor x)
        {
            var residual = fc1.Forward(x);

            var x1 = fc2.ForwardWithRelu(residual);
            x = fc3.ForwardWithRelu(x1);
            x = fc4.ForwardWithRelu(x, x1);
            x = fc5.ForwardWithRelu(x, residual);

            x = fc6.Forward(x);

            return x;
        }

        public void TrainWithLoss(Tensor loss)
        {
            optimizer.zero_grad();
            loss.backward();
            optimizer.step();
        }
        public void Save(string path) => save(path);
        public void Load(string path) => load(path);
    }
}
