using System.IO;
using System.Linq;
using AElf.Cryptography;
using AElf.Cryptography.ECDSA;
using AElf.Kernel;
using AElf.Kernel.SmartContract;
using AElf.Standards.ACS0;
using AElf.Testing.TestBase;
using AElf.Types;
using Google.Protobuf;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace AElf.Contracts.RandomContract
{
    // The Module class load the context required for unit testing
    public class Module : ContractTestModule<RandomContract>
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<ContractOptions>(o => o.ContractDeploymentAuthorityRequired = false);
            base.ConfigureServices(context);
        }
    }
    
    // The TestBase class inherit ContractTestBase class, it defines Stub classes and gets instances required for unit testing
    public class TestBase : ContractTestBase<Module>
    {
        // The Stub class for unit testing
        internal readonly RandomContractContainer.RandomContractStub RandomContractStub;
        internal readonly RandomContractContainer.RandomContractStub OtherRandomContractStub;

        internal readonly Address RandomContractAddress;
        // A key pair that can be used to interact with the contract instance
        private ECKeyPair DefaultKeyPair => Accounts[0].KeyPair;
        protected Address DefaultAddress => Accounts[0].Address;
        
        protected Address OtherAddress => Accounts[1].Address;
        private ECKeyPair OtherKeyPair => Accounts[1].KeyPair;

        public TestBase()
        {
            var zeroContractStub = GetGenesisContractContractStub(DefaultKeyPair);

            var code = ByteString.CopyFrom(File.ReadAllBytes(typeof(RandomContract).Assembly.Location).Concat(new byte[]{0x20}).ToArray());
            // var contractOperation = new ContractOperation
            // {
            //     ChainId = 9992731,
            //     CodeHash = HashHelper.ComputeFrom(code.ToByteArray()),
            //     Deployer = DefaultAddress,
            //     Salt = HashHelper.ComputeFrom("RandomContract"),
            //     Version = 1
            // };
            // contractOperation.Signature = GenerateContractSignature(DefaultKeyPair.PrivateKey, contractOperation);

            var result = AsyncHelper.RunSync(async () => await zeroContractStub.DeploySmartContract.SendAsync(
                new ContractDeploymentInput
                {
                    Category = KernelConstants.CodeCoverageRunnerCategory,
                    Code = code,
                    // ContractOperation = contractOperation
                }));
            
            RandomContractAddress = Address.Parser.ParseFrom(result.TransactionResult.ReturnValue);
            
            RandomContractStub = GetRandomContractContractStub(DefaultKeyPair);
            OtherRandomContractStub = GetRandomContractContractStub(OtherKeyPair);
        }

        private RandomContractContainer.RandomContractStub GetRandomContractContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<RandomContractContainer.RandomContractStub>(RandomContractAddress, senderKeyPair);
        }
        
        private ACS0Container.ACS0Stub GetGenesisContractContractStub(ECKeyPair senderKeyPair)
        {
            return GetTester<ACS0Container.ACS0Stub>(BasicContractZeroAddress, senderKeyPair);
        }
        
        private ByteString GenerateContractSignature(byte[] privateKey, ContractOperation contractOperation)
        {
            var dataHash = HashHelper.ComputeFrom(contractOperation);
            var signature = CryptoHelper.SignWithPrivateKey(privateKey, dataHash.ToByteArray());
            return ByteStringHelper.FromHexString(signature.ToHex());
        }
    }
    
}