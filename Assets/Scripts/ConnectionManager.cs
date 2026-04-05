using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{
    private string baseUrl = "http://127.0.0.1:8080";

    [Header("UI - Autenticación")]
    public TMP_InputField inputUsername;
    public TMP_InputField inputPassword;
    public TMP_InputField inputOldPassword; 
    public TMP_InputField inputNuevaPassword; 
    public Button botonLogin;
    public Button botonRegistro;
    public Button botonEditarPassword; 

    [Header("UI - Funciones Laborales")]
    public Button botonHorarios; 
    public Button botonNomina; 
    public TextMeshProUGUI textoMensaje;

    [System.Serializable]
    public class DatosUsuario { public string username; public string password; }

    [System.Serializable]
    public class DatosEdicion { public string username; public string oldPassword; public string newPassword; }

    
    [System.Serializable]
    public class RespuestaServidor { 
        public bool success; 
        public string mensaje; 
        public string Title; 
    }

    void Start()
    {
        if (textoMensaje != null) textoMensaje.text = "Esperando acción...";

        if (botonLogin != null) botonLogin.onClick.AddListener(IntentarLogin);
        if (botonRegistro != null) botonRegistro.onClick.AddListener(IntentarRegistro);
        if (botonEditarPassword != null) botonEditarPassword.onClick.AddListener(EditarPassword);
        if (botonHorarios != null) botonHorarios.onClick.AddListener(ConsultarHorarios);
        if (botonNomina != null) botonNomina.onClick.AddListener(ConsultarNomina);
    }

    public void IntentarLogin() => StartCoroutine(EnviarLogin("/inicioSesion", inputUsername.text, inputPassword.text, true));
    public void IntentarRegistro() => StartCoroutine(EnviarLogin("/registro", inputUsername.text, inputPassword.text, false));
    public void EditarPassword() => StartCoroutine(EnviarEdicion("/editarPassword", inputUsername.text, inputOldPassword.text, inputNuevaPassword.text));

    
    public void ConsultarHorarios() => StartCoroutine(PostRequest("/datosHorarios", "{}", false));
    public void ConsultarNomina() => StartCoroutine(PostRequest("/datosNomina", "{}", false));

    IEnumerator EnviarLogin(string ruta, string user, string pass, bool esLogin)
    {
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass)) {
            MostrarMensaje("Faltan datos", Color.yellow); yield break;
        }
        string json = JsonUtility.ToJson(new DatosUsuario { username = user, password = pass });
        yield return PostRequest(ruta, json, esLogin);
    }

    IEnumerator EnviarEdicion(string ruta, string user, string oldPass, string newPass)
    {
        string json = JsonUtility.ToJson(new DatosEdicion { username = user, oldPassword = oldPass, newPassword = newPass });
        yield return PostRequest(ruta, json, false);
    }

    IEnumerator PostRequest(string ruta, string json, bool cargarEscena)
    {
        using (UnityWebRequest request = new UnityWebRequest(baseUrl + ruta, "POST"))
        {
            byte[] body = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                MostrarMensaje("Error: No se pudo conectar al servidor", Color.red);
            }
            else
            {
                ProcesarRespuesta(request, cargarEscena);
            }
        }
    }

    void ProcesarRespuesta(UnityWebRequest request, bool cargarEscena)
    {
        try 
        {
            // Extraemos el texto JSON
            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Respuesta del servidor: " + jsonResponse);

            RespuestaServidor resp = JsonUtility.FromJson<RespuestaServidor>(jsonResponse);
            
            string mensajeAMostrar = "";

            if (!string.IsNullOrEmpty(resp.mensaje)) mensajeAMostrar = resp.mensaje;
            else if (!string.IsNullOrEmpty(resp.Title)) mensajeAMostrar = resp.Title;
            else mensajeAMostrar = "Acción realizada";

            if (request.responseCode == 200) {
                MostrarMensaje(mensajeAMostrar, Color.green);
                if (cargarEscena && resp.success) StartCoroutine(CambiarEscena());
            } else {
                MostrarMensaje(mensajeAMostrar, Color.red);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al procesar JSON: " + e.Message);
            MostrarMensaje("Error en formato de respuesta", Color.red);
        }
    }

    IEnumerator CambiarEscena() { yield return new WaitForSeconds(1.5f); SceneManager.LoadScene("Home"); }
    
    void MostrarMensaje(string msg, Color col) { 
        if(textoMensaje != null) {
            textoMensaje.text = msg; 
            textoMensaje.color = col; 
        }
    }
}