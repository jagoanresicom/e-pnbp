<%@ Page Language="C#" AutoEventWireup="true" %>
<%@ Import Namespace="System.Security.Cryptography" %>
<%@ Import Namespace="System.Threading" %>

<script runat="server">
   void Page_Load() {
      byte[] delay = new byte[1];
      RandomNumberGenerator prng = new RNGCryptoServiceProvider();

      prng.GetBytes(delay);
      Thread.Sleep((int)delay[0]);
        
      IDisposable disposable = prng as IDisposable;
      if (disposable != null) { disposable.Dispose(); }
    }
</script>

<html>
<head runat="server">
    <title>Terjadi Kesalahan</title>
</head>
<body>
    <div>
        Kami melacak kesalahan ini secara otomatis, tetapi jika masalah tetap ada jangan ragu untuk menghubungi kami. Sementara itu, cobalah refresh halaman.
    </div>
</body>
</html>