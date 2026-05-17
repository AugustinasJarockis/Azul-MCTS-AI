using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace AzulAIAndGameState.Players.MCTS_NN.Networks
{
    public class SmallConvValueNetwork : Module, INetwork
    {
        public Adam optimizer;
        public Device Device { get; private set; }

        private Layer fc1;
        private ConvolutionalLayer fc2;
        private ConvolutionalLayer fc3;
        private ConvolutionalLayer fc4;
        private ConvolutionalLayer fc5;
        private Layer fc6;

        public SmallConvValueNetwork(string modelPath = "") : base("SmallConvValueOnlyNetwork")
        {
            if (cuda.is_available())
                Device = CUDA;
            else
                Device = CPU;

            fc1 = new Layer(301, 512);
            fc2 = new ConvolutionalLayer(3);
            fc3 = new ConvolutionalLayer(3);
            fc4 = new ConvolutionalLayer(3);
            fc5 = new ConvolutionalLayer(3);
            fc6 = new Layer(512, 1);

            RegisterComponents();

            if (modelPath != "")
            {
                load(modelPath).to(Device);
            }
            optimizer = optim.Adam(parameters(), lr: 0.001);
        }

        public Tensor Call(Tensor x)
        {
            var x0 = fc1.Forward(x);

            x0 = x0.reshape(x0.shape[0], 1, 32, 16);

            var x1 = fc2.ForwardWithRelu(x0);
            x = fc3.ForwardWithRelu(x1);
            x = fc4.ForwardWithRelu(x, x0);
            x = fc5.ForwardWithRelu(x, x1);

            x = x.flatten(start_dim: 1);
            x = fc6.Forward(x);
            
            x = x.tanh();

            return x;
        }

        public void TrainWithLoss(Tensor loss)
        {
            optimizer.zero_grad();
            float lossValue = loss.item<float>();
            loss.backward();
            optimizer.step();
        }
        public void Save(string path) => save(path);
        public void Load(string path) => load(path);
    }
}
