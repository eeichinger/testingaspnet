<%@ Page Language="C#" %><% 
if (Request["testparam"] != null)
{
    Response.Write("testparam="+Request["testparam"]);
}
else
{
	Session["testkey"] = "testvalue";
    Response.Write("Hi, I'm the testpage!");
}
%>