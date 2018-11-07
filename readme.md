## DataDog Delegating Handler Aggreagate Exception Repro


* run web project.  
* Do a get of /api/values.
* should return 

```xml
<ArrayOfstring xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
	<string>correct Exception was thrown</string>
</ArrayOfstring>
```
* with the data dog .net trace agent running. An aggregate exception is trhown.

```xml
<ArrayOfstring xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
	<string>with datadog trace running an AggregateException is thrown</string>
</ArrayOfstring>
```

### DelegatingHandler 

```csharp
public class MyDelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        HttpResponseMessage retVal = null;

        try
        {
            retVal = await base.SendAsync(request, cancellationToken);
        }
        //should hit this branch
        catch (MyException)
        {
            retVal = request.CreateResponse(HttpStatusCode.OK, new string[] { "correct Exception was thrown" });
        }
        //should not hit this branch.
        catch (AggregateException)
        {
            retVal = request.CreateResponse(HttpStatusCode.OK, new string[] { "with datadog trace running an AggregateException is thrown" });
        }
        return retVal;
    }
}

```

### Excpetion Handler
```csharp
public class MyExceptionHandler : ExceptionHandler
{
    public override bool ShouldHandle(ExceptionHandlerContext context)
    {
        return false;
    }
}
```
### configuration
```csharp
 GlobalConfiguration.Configuration.MessageHandlers.Add(new MyDelegatingHandler());
 GlobalConfiguration.Configuration.Services.Replace(typeof(IExceptionHandler), new MyExceptionHandler());
```

### controller

```csharp
public IEnumerable<string> Get()
{
    throw new MyException();
}
```






```csharp
	public class MyDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage retVal = null;

            try
            {
                retVal = await base.SendAsync(request, cancellationToken);
            }
            //should hit this branch
            catch (MyException)
            {
               retVal = request.CreateResponse(HttpStatusCode.OK, new string[] { "correct Exception was thrown" });
            }
            //should not hit this branch.
            catch (AggregateException)
            {
                retVal = request.CreateResponse(HttpStatusCode.OK, new string[] { "with datadog trace running an AggregateException is thrown" });
            }
            return retVal;
        }
    }

```

### steps to repo
* edit the 'redisCon' value in the web.config ( tested with 3 node cluster)
* build and run the webapi
* navigate to /api/values
* should return

```xml
<ArrayOfstring xmlns="http://schemas.microsoft.com/2003/10/Serialization/Arrays" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
   <string>value1</string>
   <string>value2</string>
   <string>correct result</string>
   <string>result:StackExchange.Redis.HashEntry[], size is:0,isnull:False</string>
</ArrayOfstring>
```
the result we see if AMP 

```xml
<ArrayOfstring xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="http://schemas.microsoft.com/2003/10/Serialization/Arrays">
<string>value1</string>
<string>value2</string>
<string>apm oops</string>
<string>result:, size is:,isnull:True</string>
</ArrayOfstring>
```