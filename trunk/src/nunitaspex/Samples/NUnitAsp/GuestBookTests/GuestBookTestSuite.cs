using System.Diagnostics;
using NUnit.Extensions.Asp;
using NUnit.Extensions.Asp.AspTester;
using NUnit.Framework;
using NUnitAspEx;

namespace GuestBookTests
{
  [AspTestFixture( VirtualPath="/",RelativePhysicalPath="../../../GuestBook" )]
  public class GuestBookTestSuite:WebFormTestCase
  {
    private TextBoxTester name;
    private TextBoxTester comments;
    private ButtonTester save;
    private DataGridTester book;

    public GuestBookTestSuite()
    {
      Trace.WriteLine( "Created GuestBookTests fixture" );
    }

    protected override void SetUp()
    {
      name = new TextBoxTester( "name",CurrentWebForm );
      comments = new TextBoxTester( "comments",CurrentWebForm );
      save = new ButtonTester( "save",CurrentWebForm );
      book = new DataGridTester( "book",CurrentWebForm );

      // note new pseudo-protocol "asptest" here:
      //Browser.GetPage("http://localhost/NUnitAsp/sample/tutorial/GuestBook/GuestBook.aspx");
      Browser.GetPage( "asptest://localhost/GuestBook.aspx" );
    }

    [Test]
    public void TestLayout()
    {
      AssertVisibility( name,true );
      AssertVisibility( comments,true );
      AssertVisibility( save,true );
      AssertVisibility( book,false );
    }

    [Test]
    public void TestSave()
    {
      SignGuestBook( "Dr. Seuss","One Guest, Two Guest!  Guest Book, Best Book!" );

      AssertEquals( "name","",name.Text );
      AssertEquals( "comments","",comments.Text );

      string[][] expected = new string[][]
			{
				new string[] {"Dr. Seuss", "One Guest, Two Guest!  Guest Book, Best Book!"}
			};
      AssertEquals( "book",expected,book.TrimmedCells );
    }

    [Test]
    public void TestSaveTwoItems()
    {
      SignGuestBook( "Dr. Seuss","One Guest, Two Guest!  Guest Book, Best Book!" );
      SignGuestBook( "Dr. Freud","That's quite a slip you have there." );

      string[][] expected = new string[][]
			{
				new string[] {"Dr. Seuss", "One Guest, Two Guest!  Guest Book, Best Book!"},
				new string[] {"Dr. Freud", "That's quite a slip you have there."}
			};
      AssertEquals( "book",expected,book.TrimmedCells );
    }

    private void SignGuestBook(string nameToSign,string commentToSign)
    {
      name.Text = nameToSign;
      comments.Text = commentToSign;
      save.Click();
    }

  }
}
