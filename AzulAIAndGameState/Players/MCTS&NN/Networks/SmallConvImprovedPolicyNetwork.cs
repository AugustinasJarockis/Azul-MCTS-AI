using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace AzulAIAndGameState.Players.MCTS_NN.Networks
{
    public class SmallConvImprovedPolicyNetwork : Module, INetwork
    {
        public Adam optimizer;
        public Device Device { get; private set; }

        private Layer fc1;
        private ImprovedConvolutionalLayer fc2;
        private ImprovedConvolutionalLayer fc3;
        private ImprovedConvolutionalLayer fc4;
        private ImprovedConvolutionalLayer fc5;
        private Layer fc6;

        public SmallConvImprovedPolicyNetwork(string modelPath = "") : base("SmallConvImprovedPolicyNetwork")
        {
            if (cuda.is_available())
                Device = CUDA;
            else
                Device = CPU;

            fc1 = new Layer(301, 512);
            fc2 = new ImprovedConvolutionalLayer(3, 1, 5);
            fc3 = new ImprovedConvolutionalLayer(3, 5, 5);
            fc4 = new ImprovedConvolutionalLayer(3, 5, 5);
            fc5 = new ImprovedConvolutionalLayer(3, 5, 1);
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
            x = fc1.Forward(x);

            x = x.reshape(x.shape[0], 1, 32, 16);

            var x1 = fc2.ForwardWithRelu(x);
            x = fc3.ForwardWithRelu(x1);
            x = fc4.ForwardWithRelu(x, x1);
            x = fc5.ForwardWithRelu(x);

            x = x.flatten(start_dim: 1);
            x = fc6.Forward(x);

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


    internal class ImprovedConvolutionalLayer : Module
    {
        private Conv2d fc;

        public ImprovedConvolutionalLayer(int kernelSize, int inputChannels, int outputChannels) : base("LinearWrapper")
        {
            fc = Conv2d(inputChannels, outputChannels, kernelSize, stride: 1, padding: 1);

            RegisterComponents();
        }

        public Tensor Forward(Tensor x)
        {
            x = fc.forward(x);
            return x;
        }
        public Tensor Forward(Tensor x, Tensor residual)
        {
            x = fc.forward(x);
            x = x + residual;
            return x;
        }
        public Tensor ForwardWithRelu(Tensor x)
        {
            x = fc.forward(x);
            x = functional.relu(x);
            return x;
        }
        public Tensor ForwardWithRelu(Tensor x, Tensor residual)
        {
            x = fc.forward(x);
            x = x + residual;
            x = functional.relu(x);
            return x;
        }
    }
}
