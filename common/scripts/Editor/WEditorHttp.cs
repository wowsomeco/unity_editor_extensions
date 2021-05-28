using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Wowsome {
  [Serializable]
  public class UploadResult {
    public string url;
  }

  /// <summary>
  /// Http helpers for Editor.
  ///
  /// DO NOT use this for production as async await logic might not work properly on Unity mobile.
  /// </summary>
  public static class EditorHttp {
    /// <summary>
    /// Http Request for downloading data from Web Server on Editor.
    /// It downloads the data and save it to the persistent data path on success.    
    /// </summary>
    public static async Task<bool> DownloadImage(string url, string filePath) {
      string localPath = Path.Combine(Application.persistentDataPath, filePath.LastSplit());
      if (!File.Exists(localPath)) {
        WebRequest request = WebRequest.Create(Path.Combine(url, filePath));
        using (WebResponse response = await request.GetResponseAsync()) {
          using (BinaryReader reader = new BinaryReader(response.GetResponseStream())) {
            byte[] bytes = await reader.ReadAllBytes();
            File.WriteAllBytes(localPath, bytes);
            return true;
          }
        }
      }
      return false;
    }

    public static async Task<T> DownloadJson<T>(string url) {
      WebRequest request = WebRequest.Create(url);
      using (WebResponse response = await request.GetResponseAsync()) {
        using (StreamReader reader = new StreamReader(response.GetResponseStream())) {
          string str = reader.ReadToEnd();
          return JsonUtility.FromJson<T>(str);
        }
      }
    }

    public static async Task<T> Upload<T>(string url, Dictionary<string, object> parameters) {
      HttpWebResponse webResponse = await FormUpload.MultipartFormDataPost(url, "", parameters);
      // Process response
      using (StreamReader reader = new StreamReader(webResponse.GetResponseStream())) {
        string resp = await reader.ReadToEndAsync();
        T result = JsonUtility.FromJson<T>(resp);
        return result;
      }
    }

    public static async Task<byte[]> ReadAllBytes(this BinaryReader reader) {
      const int bufferSize = 4096;
      using (MemoryStream ms = new MemoryStream()) {
        byte[] buffer = new byte[bufferSize];
        int count;
        while ((count = reader.Read(buffer, 0, buffer.Length)) != 0) {
          await ms.WriteAsync(buffer, 0, count);
        }
        return ms.ToArray();
      }
    }
  }

  // Implements multipart/form-data POST in C# http://www.ietf.org/rfc/rfc2388.txt
  // http://www.briangrinstead.com/blog/multipart-form-post-in-c
  public static class FormUpload {
    private static readonly Encoding encoding = Encoding.UTF8;
    public static async Task<HttpWebResponse> MultipartFormDataPost(string postUrl, string userAgent, Dictionary<string, object> postParameters) {
      string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
      string contentType = "multipart/form-data; boundary=" + formDataBoundary;

      byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

      return await PostForm(postUrl, userAgent, contentType, formData);
    }

    private static async Task<HttpWebResponse> PostForm(string postUrl, string userAgent, string contentType, byte[] formData) {
      HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

      if (request == null) {
        throw new NullReferenceException("request is not a http request");
      }

      // Set up the request properties.
      request.Method = "POST";
      request.ContentType = contentType;
      request.UserAgent = userAgent;
      request.CookieContainer = new CookieContainer();
      request.ContentLength = formData.Length;

      // Send the form data to the request.
      using (Stream requestStream = await request.GetRequestStreamAsync()) {
        requestStream.Write(formData, 0, formData.Length);
        requestStream.Close();
      }

      return request.GetResponse() as HttpWebResponse;
    }

    private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary) {
      Stream formDataStream = new MemoryStream();
      bool needsCLRF = false;

      foreach (var param in postParameters) {
        // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
        // Skip it on the first parameter, add it to subsequent parameters.
        if (needsCLRF)
          formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

        needsCLRF = true;

        if (param.Value is FileParameter) {
          FileParameter fileToUpload = (FileParameter)param.Value;

          // Add just the first part of this param, since we will write the file data directly to the Stream
          string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n",
              boundary,
              param.Key,
              fileToUpload.FileName ?? param.Key,
              fileToUpload.ContentType ?? "application/octet-stream");

          formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

          // Write the file data directly to the Stream, rather than serializing it to a string.
          formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
        } else {
          string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
              boundary,
              param.Key,
              param.Value);
          formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
        }
      }

      // Add the end of the request.  Start with a newline
      string footer = "\r\n--" + boundary + "--\r\n";
      formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

      // Dump the Stream into a byte[]
      formDataStream.Position = 0;
      byte[] formData = new byte[formDataStream.Length];
      formDataStream.Read(formData, 0, formData.Length);
      formDataStream.Close();

      return formData;
    }

    public class FileParameter {
      public byte[] File { get; set; }
      public string FileName { get; set; }
      public string ContentType { get; set; }
      public FileParameter(byte[] file) : this(file, null) { }
      public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
      public FileParameter(byte[] file, string filename, string contenttype) {
        File = file;
        FileName = filename;
        ContentType = contenttype;
      }
    }
  }
}