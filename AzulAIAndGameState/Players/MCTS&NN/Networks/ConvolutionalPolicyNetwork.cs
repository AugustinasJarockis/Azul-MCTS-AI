using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace AzulAIAndGameState.Players.MCTS_NN.Networks
{
    public class ConvolutionalPolicyNetwork : Module, INetwork
    {
        public Adam optimizer;
        public Device Device { get; private set; }

        private Layer fc1;
        private ConvolutionalLayer fc2;
        private ConvolutionalLayer fc3;
        private ConvolutionalLayer fc4;
        private ConvolutionalLayer fc5;
        private ConvolutionalLayer fc6;
        private ConvolutionalLayer fc7;
        private ConvolutionalLayer fc8;
        private ConvolutionalLayer fc9;
        private Layer fc10;
        private ConvolutionalLayer fc11;
        private ConvolutionalLayer fc12;
        private ConvolutionalLayer fc13;
        private ConvolutionalLayer fc14;

        public ConvolutionalPolicyNetwork(string modelPath = "") : base("ConvolutionalPolicyOnlyNetwork")
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
            fc6 = new ConvolutionalLayer(3);
            fc7 = new ConvolutionalLayer(3);
            fc8 = new ConvolutionalLayer(3);
            fc9 = new ConvolutionalLayer(3);
            fc11 = new ConvolutionalLayer(3);
            fc12 = new ConvolutionalLayer(3);
            fc13 = new ConvolutionalLayer(3);
            fc14 = new ConvolutionalLayer(3);
            fc10 = new Layer(512, 180);

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

            residual = residual.reshape(residual.shape[0], 1, 32, 16);

            var x1 = fc2.ForwardWithRelu(residual);
            var x2 = fc3.ForwardWithRelu(x1);
            var x3 = fc4.ForwardWithRelu(x2);
            var x4 = fc5.ForwardWithRelu(x3);
            x = fc6.ForwardWithRelu(x4, residual);

            x = fc6.ForwardWithRelu(x, x4);
            x = fc7.ForwardWithRelu(x, x3);
            x = fc8.ForwardWithRelu(x, x2);
            x = fc9.ForwardWithRelu(x, x1);

            x = fc11.ForwardWithRelu(x, x4);
            x = fc12.ForwardWithRelu(x, x3);
            x = fc13.ForwardWithRelu(x, x2);
            x = fc14.ForwardWithRelu(x, x1);

            x = x.flatten(start_dim: 1);
            x = fc10.Forward(x);

            return x;
        }

        public void TrainWithLoss(Tensor loss)
        {
            optimizer.zero_grad();
            float lossValue = loss.item<float>();
            //Console.WriteLine("Backpropagating: " + lossValue);
            loss.backward();
            optimizer.step();
        }
        public void Save(string path) => save(path);
        public void Load(string path) => load(path);
    }

    internal class ConvolutionalLayer : Module
    {
        private Conv2d fc;

        public ConvolutionalLayer(int kernelSize) : base("LinearWrapper")
        {
            fc = Conv2d(1, 1, kernelSize, stride: 1, padding: 1);

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
