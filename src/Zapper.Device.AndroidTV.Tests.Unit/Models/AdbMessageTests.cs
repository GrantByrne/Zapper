using AwesomeAssertions;
using Zapper.Device.AndroidTV.Models;

namespace Zapper.Device.AndroidTV.Tests.Unit.Models;

public class AdbMessageTests
{
    [Fact]
    public void ToBytes_ValidMessage_CreatesCorrectByteArray()
    {
        // Arrange
        var message = new AdbMessage
        {
            Command = 0x01234567,
            Arg0 = 0x89ABCDEF,
            Arg1 = 0xFEDCBA98,
            DataLength = 0,
            DataCrc32 = 0x76543210,
            Magic = 0x12345678
        };

        // Act
        var bytes = message.ToBytes();

        // Assert
        bytes.Length.Should().Be((int)AdbMessage.AdbHeaderLength);

        BitConverter.ToUInt32(bytes, 0).Should().Be(message.Command);
        BitConverter.ToUInt32(bytes, 4).Should().Be(message.Arg0);
        BitConverter.ToUInt32(bytes, 8).Should().Be(message.Arg1);
        BitConverter.ToUInt32(bytes, 12).Should().Be(message.DataLength);
        BitConverter.ToUInt32(bytes, 16).Should().Be(message.DataCrc32);
        BitConverter.ToUInt32(bytes, 20).Should().Be(message.Magic);
    }

    [Fact]
    public void ToBytes_MessageWithData_IncludesDataInByteArray()
    {
        // Arrange
        var testData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
        var message = new AdbMessage
        {
            Command = AdbCommands.Write,
            Arg0 = 1,
            Arg1 = 2,
            DataLength = (uint)testData.Length,
            DataCrc32 = 0,
            Magic = AdbCommands.Write ^ 0xffffffff,
            Data = testData
        };

        // Act
        var bytes = message.ToBytes();

        // Assert
        bytes.Length.Should().Be((int)(AdbMessage.AdbHeaderLength + testData.Length));

        // Check header
        BitConverter.ToUInt32(bytes, 12).Should().Be((uint)testData.Length);

        // Check data
        for (int i = 0; i < testData.Length; i++)
        {
            bytes[AdbMessage.AdbHeaderLength + i].Should().Be(testData[i]);
        }
    }

    [Fact]
    public void FromBytes_ValidHeaderBytes_CreatesCorrectMessage()
    {
        // Arrange
        var originalMessage = new AdbMessage
        {
            Command = 0x01234567,
            Arg0 = 0x89ABCDEF,
            Arg1 = 0xFEDCBA98,
            DataLength = 0,
            DataCrc32 = 0x76543210,
            Magic = 0x12345678
        };
        var bytes = originalMessage.ToBytes();

        // Act
        var parsedMessage = AdbMessage.FromBytes(bytes);

        // Assert
        parsedMessage.Command.Should().Be(originalMessage.Command);
        parsedMessage.Arg0.Should().Be(originalMessage.Arg0);
        parsedMessage.Arg1.Should().Be(originalMessage.Arg1);
        parsedMessage.DataLength.Should().Be(originalMessage.DataLength);
        parsedMessage.DataCrc32.Should().Be(originalMessage.DataCrc32);
        parsedMessage.Magic.Should().Be(originalMessage.Magic);
        parsedMessage.Data.Should().BeEmpty();
    }

    [Fact]
    public void FromBytes_BytesWithData_ParsesDataCorrectly()
    {
        // Arrange
        var testData = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0xEE };
        var originalMessage = new AdbMessage
        {
            Command = AdbCommands.Write,
            Arg0 = 1,
            Arg1 = 2,
            DataLength = (uint)testData.Length,
            DataCrc32 = 0,
            Magic = AdbCommands.Write ^ 0xffffffff,
            Data = testData
        };
        var bytes = originalMessage.ToBytes();

        // Act
        var parsedMessage = AdbMessage.FromBytes(bytes);

        // Assert
        parsedMessage.DataLength.Should().Be((uint)testData.Length);
        parsedMessage.Data.Should().Equal(testData);
    }

    [Fact]
    public void FromBytes_BufferTooSmall_ThrowsArgumentException()
    {
        // Arrange
        var smallBuffer = new byte[AdbMessage.AdbHeaderLength - 1];

        // Act & Assert
        Assert.Throws<ArgumentException>(() => AdbMessage.FromBytes(smallBuffer));
    }

    [Fact]
    public void FromBytes_BufferWithoutFullData_CreatesMessageWithoutData()
    {
        // Arrange
        var message = new AdbMessage
        {
            Command = AdbCommands.Write,
            Arg0 = 1,
            Arg1 = 2,
            DataLength = 100, // Claims 100 bytes of data
            DataCrc32 = 0,
            Magic = AdbCommands.Write ^ 0xffffffff
        };
        var headerBytes = new byte[AdbMessage.AdbHeaderLength];
        BitConverter.GetBytes(message.Command).CopyTo(headerBytes, 0);
        BitConverter.GetBytes(message.Arg0).CopyTo(headerBytes, 4);
        BitConverter.GetBytes(message.Arg1).CopyTo(headerBytes, 8);
        BitConverter.GetBytes(message.DataLength).CopyTo(headerBytes, 12);
        BitConverter.GetBytes(message.DataCrc32).CopyTo(headerBytes, 16);
        BitConverter.GetBytes(message.Magic).CopyTo(headerBytes, 20);

        // Act
        var parsedMessage = AdbMessage.FromBytes(headerBytes);

        // Assert
        parsedMessage.DataLength.Should().Be(100);
        parsedMessage.Data.Should().BeEmpty(); // No data was parsed because buffer was too small
    }

    [Fact]
    public void RoundTrip_MessageWithAllFields_PreservesAllData()
    {
        // Arrange
        var originalMessage = new AdbMessage
        {
            Command = AdbCommands.Open,
            Arg0 = 42,
            Arg1 = 84,
            DataLength = 11,
            DataCrc32 = 0x12345678,
            Magic = AdbCommands.Open ^ 0xffffffff,
            Data = System.Text.Encoding.UTF8.GetBytes("Hello World")
        };

        // Act
        var bytes = originalMessage.ToBytes();
        var parsedMessage = AdbMessage.FromBytes(bytes);

        // Assert
        parsedMessage.Command.Should().Be(originalMessage.Command);
        parsedMessage.Arg0.Should().Be(originalMessage.Arg0);
        parsedMessage.Arg1.Should().Be(originalMessage.Arg1);
        parsedMessage.DataLength.Should().Be(originalMessage.DataLength);
        parsedMessage.DataCrc32.Should().Be(originalMessage.DataCrc32);
        parsedMessage.Magic.Should().Be(originalMessage.Magic);
        parsedMessage.Data.Should().Equal(originalMessage.Data);
    }
}