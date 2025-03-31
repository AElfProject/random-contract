using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.CSharp.Core;
using AElf.CSharp.Core.Extension;
using AElf.Types;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;

namespace AElf.Contracts.RandomContract;

public class RandomContractTests : TestBase
{
    [Fact]
    public async Task InitializeContractTest()
    {
        var maxValueLimit = 10000;
        var maxRandomNumberCount = 100;
        
        // Initialize the contract with unauthorized account and expect an exception
        var result = await OtherRandomContractStub.Initialize.SendWithExceptionAsync(new InitializeInput
        {
            MaxValueLimit = maxValueLimit,
            MaxRandomNumberCount = maxRandomNumberCount
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("No permission");
        
        // Initialize the contract successfully
        result = await InitializeContractAsync(maxValueLimit, maxRandomNumberCount);
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
        
        // Check if admin is set correctly
        var adminResult = await RandomContractStub.GetAdmin.CallAsync(new Empty());
        adminResult.ShouldBe(DefaultAddress);
        
        // Check if max value limit is set correctly
        var maxValueLimitResult = await RandomContractStub.GetMaxValueLimit.CallAsync(new Empty());
        maxValueLimitResult.Value.ShouldBe(maxValueLimit);
        
        // Check if max random number count is set correctly
        var maxRandomNumberCountResult = await RandomContractStub.GetMaxRandomNumberCount.CallAsync(new Empty());
        maxRandomNumberCountResult.Value.ShouldBe(maxRandomNumberCount);
            
        // Initialize the contract again and expect an exception
        result = await RandomContractStub.Initialize.SendWithExceptionAsync(new InitializeInput
        {
            MaxValueLimit = maxValueLimit,
            MaxRandomNumberCount = maxRandomNumberCount
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("Already initialized");
    }
    
    [Fact]
    public async Task SetAdminTest()
    {
        // Initialize the contract
        var maxValueLimit = 10000;
        var maxRandomNumberCount = 100;
        await InitializeContractAsync(maxValueLimit, maxRandomNumberCount);
        
        // Change admin
        await RandomContractStub.SetAdmin.SendAsync(OtherAddress);
        var admin = await RandomContractStub.GetAdmin.CallAsync(new Empty());
        admin.ShouldBe(OtherAddress);
        
        // Set admin with an unauthorized account and expect an exception
        var result = await RandomContractStub.SetAdmin.SendWithExceptionAsync(DefaultAddress);
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("Only admin can perform this action");
        
        // Set admin with invalid address and expect an exception
        result = await OtherRandomContractStub.SetAdmin.SendWithExceptionAsync(new Address());
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("Invalid admin address");
        
        // Set admin with an authorized account
        result = await OtherRandomContractStub.SetAdmin.SendAsync(DefaultAddress);
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
        admin = await OtherRandomContractStub.GetAdmin.CallAsync(new Empty());
        admin.ShouldBe(DefaultAddress);
    }
    
    [Fact]
    public async Task SetMaxValueLimitTest()
    {
        // Initialize the contract
        var maxValueLimit = 10000;
        var maxRandomNumberCount = 100;
        await InitializeContractAsync(maxValueLimit, maxRandomNumberCount);
        
        // Change max value limit
        var newMaxValueLimit = 1000;
        await RandomContractStub.SetMaxValueLimit.SendAsync(new Int32Value
        {
            Value = newMaxValueLimit
        });
        var maxValueLimitResult = await RandomContractStub.GetMaxValueLimit.CallAsync(new Empty());
        maxValueLimitResult.Value.ShouldBe(newMaxValueLimit);
        
        // Set max value limit with an unauthorized account and expect an exception
        var result = await OtherRandomContractStub.SetMaxValueLimit.SendWithExceptionAsync(new Int32Value
        {
            Value = newMaxValueLimit
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("Only admin can perform this action");
        
        // Set max value limit with invalid value and expect an exception
        result = await RandomContractStub.SetMaxValueLimit.SendWithExceptionAsync(new Int32Value
        {
            Value = 0
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("Invalid max value limit");
    }
    
    [Fact]
    public async Task SetMaxRandomNumberCountTest()
    {
        // Initialize the contract
        var maxValueLimit = 10000;
        var maxRandomNumberCount = 100;
        await InitializeContractAsync(maxValueLimit, maxRandomNumberCount);
        
        // Change max random number count
        var newMaxRandomNumberCount = 10;
        await RandomContractStub.SetMaxRandomNumberCount.SendAsync(new Int32Value
        {
            Value = newMaxRandomNumberCount
        });
        var maxRandomNumberCountResult = await RandomContractStub.GetMaxRandomNumberCount.CallAsync(new Empty());
        maxRandomNumberCountResult.Value.ShouldBe(newMaxRandomNumberCount);
        
        // Set max random number count with an unauthorized account and expect an exception
        var result = await OtherRandomContractStub.SetMaxRandomNumberCount.SendWithExceptionAsync(new Int32Value
        {
            Value = newMaxRandomNumberCount
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("Only admin can perform this action");
        
        // Set max random number count with invalid value and expect an exception
        result = await RandomContractStub.SetMaxRandomNumberCount.SendWithExceptionAsync(new Int32Value
        {
            Value = 0
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("Invalid max random number count");
    }
        
    [Fact]
    public async Task GenerateRandomNumberTest()
    {
        // Initialize the contract
        var maxValueLimit = 10000;
        var maxRandomNumberCount = 100;
        await InitializeContractAsync(maxValueLimit, maxRandomNumberCount);
            
        // Generate a random number
        var hash = "TestHash";
        var randomNumberCount = 1;
        var maxValue = 10;
        var result = await RandomContractStub.GenerateRandomNumber.SendAsync(new GenerateRandomNumberInput
        {
            Hash = hash,
            RandomNumberCount = randomNumberCount,
            MaxValue = maxValue
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
        var randomNumberGenerated = GetRandomNumberGenerated(result.TransactionResult);
        randomNumberGenerated.RandomNumbers.Value.Count.ShouldBe(randomNumberCount);
            
        // Get the generated random number
        var randomNumberList = await RandomContractStub.GetRandomNumber.CallAsync(new GetRandomNumberInput
        {
            Hash = hash
        });
        randomNumberList.Value.Count.ShouldBe(randomNumberCount);
        var randomNumber = randomNumberList.Value[0];
        randomNumber.ShouldBe(randomNumberGenerated.RandomNumbers.Value[0]);
        randomNumber.ShouldBeLessThan(maxValue);
        
        // Generate 100 random number
        randomNumberCount = 10;
        maxValue = 100;
        hash = "Test";
        result = await RandomContractStub.GenerateRandomNumber.SendAsync(new GenerateRandomNumberInput
        {
            Hash = hash,
            RandomNumberCount = randomNumberCount,
            MaxValue = maxValue
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Mined);
        randomNumberGenerated = GetRandomNumberGenerated(result.TransactionResult);
        randomNumberGenerated.RandomNumbers.Value.Count.ShouldBe(randomNumberCount);
            
        // Get the generated random number
        randomNumberList = await RandomContractStub.GetRandomNumber.CallAsync(new GetRandomNumberInput
        {
            Hash = hash
        });
        randomNumberList.Value.Count.ShouldBe(randomNumberCount);
        var randomSet = new HashSet<int>();
        for (var i = 0; i < randomNumberCount; i++)
        {
            randomNumber = randomNumberList.Value[i];
            randomNumber.ShouldBe(randomNumberGenerated.RandomNumbers.Value[i]);
            randomNumber.ShouldBeLessThan(maxValue);
            // Check if the random number is duplicated
            randomSet.ShouldNotContain(randomNumber);
            randomSet.Add(randomNumber);
        }
        
        // Generate random number with same hash and expect an exception
        result = await RandomContractStub.GenerateRandomNumber.SendWithExceptionAsync(new GenerateRandomNumberInput
        {
            Hash = hash,
            RandomNumberCount = randomNumberCount,
            MaxValue = maxValue
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("Random number already generated");
        
        var invalidHash = "cdd6b38b763d929355c47fb6f7be5107cdd6b38b763d929355c47fb6f7be5107cdd6b38b763d929355c47fb6f7be510712345";
        // Generate random number with invalid hash length and expect an exception
        result = await RandomContractStub.GenerateRandomNumber.SendWithExceptionAsync(new GenerateRandomNumberInput
        {
            Hash = invalidHash,
            RandomNumberCount = randomNumberCount,
            MaxValue = maxValue
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("Invalid hash length");
        
        hash = "OtherHash";
        // Generate random number with invalid max value and expect an exception
        result = await RandomContractStub.GenerateRandomNumber.SendWithExceptionAsync(new GenerateRandomNumberInput
        {
            Hash = hash,
            RandomNumberCount = randomNumberCount,
            MaxValue = 0
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("Invalid max value");
        
        result = await RandomContractStub.GenerateRandomNumber.SendWithExceptionAsync(new GenerateRandomNumberInput
        {
            Hash = hash,
            RandomNumberCount = randomNumberCount,
            MaxValue = maxValueLimit + 1
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("Invalid max value");
        
        // Generate random number with invalid random number count and expect an exception
        result = await RandomContractStub.GenerateRandomNumber.SendWithExceptionAsync(new GenerateRandomNumberInput
        {
            Hash = hash,
            RandomNumberCount = 0,
            MaxValue = maxValue
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("Invalid random number count");

        result = await RandomContractStub.GenerateRandomNumber.SendWithExceptionAsync(new GenerateRandomNumberInput
        {
            Hash = hash,
            RandomNumberCount = maxRandomNumberCount + 1,
            MaxValue = maxValueLimit
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("Invalid random number count");
        
        result = await RandomContractStub.GenerateRandomNumber.SendWithExceptionAsync(new GenerateRandomNumberInput
        {
            Hash = hash,
            RandomNumberCount = maxValue + 1,
            MaxValue = maxValue
        });
        result.TransactionResult.Status.ShouldBe(TransactionResultStatus.Failed);
        result.TransactionResult.Error.ShouldContain("Invalid random number count");
    }
    
    private async Task<IExecutionResult<Empty>> InitializeContractAsync(int maxValueLimit, int maxRandomNumberCount)
    {
        var initializeInput = new InitializeInput
        {
            MaxValueLimit = maxValueLimit,
            MaxRandomNumberCount = maxRandomNumberCount
        };
        // Initialize the contract successfully
        return await RandomContractStub.Initialize.SendAsync(initializeInput);
    }
        
    private RandomNumberGenerated GetRandomNumberGenerated(TransactionResult transactionResult)
    {
        var randomNumberGeneratedLogEvent = transactionResult.Logs.FirstOrDefault(l => l.Name == "RandomNumberGenerated");
        if (randomNumberGeneratedLogEvent == null)
        {
            return null;
        }
        var randomNumberGenerated = new RandomNumberGenerated();
        randomNumberGenerated.MergeFrom(randomNumberGeneratedLogEvent);
        return randomNumberGenerated;
    }
}