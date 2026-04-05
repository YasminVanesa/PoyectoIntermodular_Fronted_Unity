using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using TMPro; // Indispensable para usar TextMeshPro
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConnectionManager : MonoBehaviour
{
    private string baseUrl = "http://127.0.0.1:8080";

    [Header("Elementos de la Interfaz (UI)")]
    public TMP_InputField inputUsername; // Cuadro para escribir el usuario
    public TMP_InputField inputPassword; // Cuadro para escribir la contraseña
    public Button botonLogin;            // Botón para iniciar sesión
    public Button botonRegistro;         // Botón para registrarse
    public TextMeshProUGUI textoMensaje; // Texto que mostrará el resultado al usuario

    // Estructura para enviar el JSON a Node.js
    [System.Serializable]
    public class DatosLogin
    {
        public string username;
        public string password;
        public string rutas;
    }

    // Estructura para leer el JSON que nos devuelve Node.js
    [System.Serializable]
    public class RespuestaLogin
    {
        public bool success;
        public string mensaje;
        public string username;
    }

    void Start()
    {
        // Limpiamos el mensaje al iniciar
        if(textoMensaje != null) textoMensaje.text = "";

        // Enlazamos el botón a la función por código (así no tienes que hacerlo manualmente en el Inspector)
        if (botonLogin != null)
        {
            botonLogin.onClick.AddListener(IntentarLogin);
        }
    }

    // Esta función se ejecuta al pulsar el botón
    public void IntentarLogin()
    {
        // Leemos lo que el usuario ha escrito en los cuadros
        string user = inputUsername.text;
        string pass = inputPassword.text;

        // Comprobamos que no estén vacíos
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            textoMensaje.text = "Por favor, rellena todos los campos.";
            textoMensaje.color = Color.yellow;
            return;
        }

        // Mostramos mensaje de carga
        textoMensaje.text = "Conectando con el servidor...";
        textoMensaje.color = Color.white;

        // Iniciamos la conexión con Node.js
        StartCoroutine(HacerLogin("/inicioSesion", user, pass));
    }

    // NUEVO: Función para el botón de Registro
    public void IntentarRegistro()
    {
        string user = inputUsername.text;
        string pass = inputPassword.text;

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            textoMensaje.text = "Rellena todos los campos para registrarte.";
            textoMensaje.color = Color.yellow;
            return;
        }

        textoMensaje.text = "Registrando usuario...";
        textoMensaje.color = Color.white;
        StartCoroutine(HacerLogin("/registro", user, pass));
    }

    IEnumerator HacerLogin(string ruta, string user, string pass)
    {
        DatosLogin datos = new DatosLogin { rutas = ruta, username = user, password = pass };
        string jsonData = JsonUtility.ToJson(datos);

        using (UnityWebRequest request = new UnityWebRequest(baseUrl + "/inicioSesion", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            // Intentamos convertir la respuesta JSON de Node en nuestro objeto RespuestaLogin
            RespuestaLogin respuesta = null;
            try
            {
                respuesta = JsonUtility.FromJson<RespuestaLogin>(request.downloadHandler.text);
            }
            catch
            {
                Debug.LogWarning("La respuesta del servidor no es un JSON válido o está vacía.");
            }

            // Analizamos el resultado de la petición
            if (request.result != UnityWebRequest.Result.Success)
            {
                // Si hay error (401 de contraseña incorrecta o servidor apagado)
                textoMensaje.color = Color.red;
                
                if (respuesta != null && !string.IsNullOrEmpty(respuesta.mensaje))
                {
                    textoMensaje.text = respuesta.mensaje; // Muestra "La contraseña no es correcta", etc.
                }
                else
                {
                    textoMensaje.text = "Error de conexión con el servidor.";
                }
            }
            else
            {
                // Si todo sale bien (Código 200)
                textoMensaje.color = Color.green;
                
                if (respuesta != null)
                {
                    textoMensaje.text = respuesta.mensaje; // Muestra "Acceso concedido. ¡Bienvenido!"
                    yield return new WaitForSeconds(1.5f); 
                    SceneManager.LoadScene("Home");
                }
            }
        }
    }
}