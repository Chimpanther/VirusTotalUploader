using Xunit;
public class DummyTest {
    [Fact] public void Test() {
        var form = new System.Windows.Forms.Form();
        Assert.NotNull(form);
    }
}
