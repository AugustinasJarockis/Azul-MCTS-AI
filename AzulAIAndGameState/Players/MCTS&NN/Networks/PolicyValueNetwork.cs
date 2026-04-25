using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;

namespace AzulBoardGame.Players.MCTS_CNN
{
    public class PolicyValueNetwork : Module
    {
        public Adam optimizer;

        private Linear fc0;
        private BatchNorm1d bn0;
        private Linear fc1;
        private BatchNorm1d bn1;
        private Linear fc2;
        private BatchNorm1d bn2;
        private Linear fc3;
        private BatchNorm1d bn3;
        private Linear fc4;
        private BatchNorm1d bn4;
        private Linear fc5;
        private BatchNorm1d bn5;
        private Linear fc6;
        private BatchNorm1d bn6;
                
        private Linear fcPolicy1;
        private BatchNorm1d bnp1;
        private Linear fcPolicy2;
        private BatchNorm1d bnp2;
        private Linear fcPolicy3;
                
        private Linear fcValue1;
        private BatchNorm1d bnv1;
        private Linear fcValue2;
        private BatchNorm1d bnv2;
        private Linear fcValue3;

        public PolicyValueNetwork(string modelPath = "") : base("PolicyNetwork") {
            //conv1 = Conv2d(134, 64, 3, stride: 1, padding: 1);
            //conv2 = Conv2d(64, 64, 3, stride: 1, padding: 1);
            //conv3 = Conv2d(64, 32, 3, stride: 1, padding: 1);
            //pool = MaxPool2d(2);
            fc0 = Linear(121, 256);
            bn0 = BatchNorm1d(256);
            fc1 = Linear(256, 256);
            bn1 = BatchNorm1d(256);
            fc2 = Linear(256, 128);
            bn2 = BatchNorm1d(128);
            fc3 = Linear(128, 64);
            bn3 = BatchNorm1d(64);
            fc4 = Linear(64, 128);
            bn4 = BatchNorm1d(128);
            fc5 = Linear(128, 256);
            bn5 = BatchNorm1d(256);
            fc6 = Linear(256, 256);
            bn6 = BatchNorm1d(256);

            fcPolicy1 = Linear(256, 256);
            bnp1 = BatchNorm1d(256);
            fcPolicy2 = Linear(256, 256);
            bnp2 = BatchNorm1d(256);
            fcPolicy3 = Linear(256, 180);

            fcValue1 = Linear(256, 128);
            bnv1 = BatchNorm1d(128);
            fcValue2 = Linear(128, 64);
            bnv2 = BatchNorm1d(64);
            fcValue3 = Linear(64, 1);

            RegisterComponents();

            if (modelPath != "") {
                load(modelPath);
            }
            optimizer = optim.Adam(parameters(), lr: 0.001);
        }

        public (Tensor, Tensor) Call(Tensor x) {
            //x = conv1.forward(x);
            //x = functional.relu(x);
            //x = pool.forward(x);

            //x = conv2.forward(x);
            //x = functional.relu(x);
            //x = pool.forward(x);

            //x = conv3.forward(x);
            //x = functional.relu(x);
            //x = pool.forward(x);

            //x = x.view(x.shape[0], -1);

            x = fc0.forward(x);
            //x = bn0.forward(x);
            x = functional.relu(x);

            var residual = x;
            x = fc1.forward(x);
            //x = bn1.forward(x);
            x = x + residual;
            x = functional.relu(x);

            var residual2 = x;
            x = fc2.forward(x);
            //x = bn2.forward(x);
            x = functional.relu(x);

            var residual3 = x;
            x = fc3.forward(x);
            x = bn3.forward(x);
            x = functional.relu(x);

            x = fc4.forward(x);
            //x = bn4.forward(x);
            x = x + residual3;
            x = functional.relu(x);

            x = fc5.forward(x);
            //x = bn5.forward(x);
            x = x + residual2;
            x = functional.relu(x);

            x = fc6.forward(x);
            //x = bn6.forward(x);
            x = x + residual;
            x = functional.relu(x);

            var xPolicy = fcPolicy1.forward(x);
            xPolicy = bnp1.forward(xPolicy);
            xPolicy = functional.relu(xPolicy);

            xPolicy = fcPolicy2.forward(xPolicy);
            xPolicy = bnp2.forward(xPolicy);
            xPolicy = functional.relu(xPolicy);

            xPolicy = fcPolicy3.forward(xPolicy);
            
            var xValue = fcValue1.forward(x);
            //xValue = bnv1.forward(xValue);
            xValue = functional.relu(xValue);

            xValue = fcValue2.forward(xValue);
            //xValue = bnv2.forward(xValue);
            xValue = functional.relu(xValue);
            
            xValue = fcValue3.forward(xValue);
            xValue = xValue.tanh();

            return (xPolicy, xValue);
        }

        public void TrainWithLoss(Tensor loss) {
            optimizer.zero_grad();
            float lossValue = loss.mean().item<float>();
            //Console.WriteLine("Backpropagating: " + lossValue);
            loss.mean().backward();
            optimizer.step();
        }
    }
}
