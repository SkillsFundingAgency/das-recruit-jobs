using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using SFA.DAS.Recruit.Jobs.Core.Configuration;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Core;

public class WhenShorteningAzureRuleName
{
    private static Type DynamicallyCreateType(string typeName)
    {
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Foo"), AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("Foo");
        var typeBuilder = moduleBuilder.DefineType(typeName , TypeAttributes.Public);
        return typeBuilder.CreateType();
    }
    
    [TestCase("This_Is_A_Class_With_A_50_Character_Long_Type_Name")]
    [TestCase("This_Type_Name_Has_Less_Than_50_Characters")]
    public void Then_The_Name_Will_Not_Be_Shortened_If_It_Is_50_Characters_Or_Less(string typeName)
    {
        // arrange
        var type = DynamicallyCreateType(typeName);
        
        // act
        var result = AzureRuleNameShortener.Shorten(type);

        // assert
        result.Should().Be(typeName);
    }
    
    [Test]
    public void Then_The_Name_Will_Be_Shortened_If_It_Is_More_Than_50_Characters()
    {
        // arrange
        const string typeName = "This_Is_A_Class_With_More_Than_50_Characters_As_Its_Type_Name";
        var type = DynamicallyCreateType(typeName);
        var bytes = System.Text.Encoding.Default.GetBytes(type.FullName!);
        var hash = MD5.HashData(bytes);
        var expectedName = new Guid(hash).ToString();

        // act
        var result = AzureRuleNameShortener.Shorten(type);

        // assert
        result.Should().Be(expectedName);
    }
}