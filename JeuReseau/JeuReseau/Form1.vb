Imports System.Net
Imports System.Net.Sockets
Imports System.Threading

Public Class Client1

    Dim port As String = "8081"
    Dim ip As String = "127.0.0.1"
    Dim id As String = "2"
    Dim emplacement As Integer = 0
    Dim emplacementAdversaire As Integer = 0
    Dim ThreadRun, ThreadEcoute As Thread
    Dim SocketClient As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
    Dim MonEP As IPEndPoint = New IPEndPoint(IPAddress.Parse(ip), port)
    Dim notConnected As Boolean
    Dim random As New Random()
    Dim casesJoueur1() As Panel
    Dim casesJoueur2() As Panel
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        'on stocke les panel correspondant au cases des joueurs dans des tableaux afin de les manipuler par la suite
        casesJoueur1 = {joueur1Depart, pan1j1, pan2j1, pan3j1, pan4j1, pan5j1, pan6j1, pan7j1, pan8j1, pan9j1, pan10j1, pan11j1, pan12j1, pan13j1, pan14j1, pan15j1, pan16j1, pan17j1, pan18j1, pan19j1, pan20j1, pan21j1, pan22j1, pan23j1, pan24j1, pan25j1, pan26j1, pan27j1, pan28j1, pan29j1, pan30j1, pan31j1, pan32j1, pan33j1, pan34j1, pan35j1, pan36j1, pan37j1, pan38j1, pan39j1, pan40j1, pan41j1, pan42j1, pan43j1, pan44j1, pan45j1, pan46j1, pan47j1, pan48j1, pan49j1, pan50j1, pan51j1, pan52j1, pan53j1, pan54j1, pan55j1, pan56j1, pan57j1, pan58j1, pan59j1, pan60j1}
        casesJoueur2 = {joueur2Depart, pan1j2, pan2j2, pan3j2, pan4j2, pan5j2, pan6j2, pan7j2, pan8j2, pan9j2, pan10j2, pan11j2, pan12j2, pan13j2, pan14j2, pan15j2, pan16j2, pan17j2, pan18j2, pan19j2, pan20j2, pan21j2, pan22j2, pan23j2, pan24j2, pan25j2, pan26j2, pan27j2, pan28j2, pan29j2, pan30j2, pan31j2, pan32j2, pan33j2, pan34j2, pan35j2, pan36j2, pan37j2, pan38j2, pan39j2, pan40j2, pan41j2, pan42j2, pan43j2, pan44j2, pan45j2, pan46j2, pan47j2, pan48j2, pan49j2, pan50j2, pan51j2, pan52j2, pan53j2, pan54j2, pan55j2, pan56j2, pan57j2, pan58j2, pan59j2, pan60j2}

        'on initialise les dés à 0
        des1.Text = "0"
        des2.Text = "0"

        'on desactive le bouton de lancer de dé en attendant l'autorisation du serveur
        Button1.Enabled = False

        'boolean signalant quand on est connecté
        notConnected = True

        Try
            SocketClient.Connect(MonEP)

            Connexion()

            ThreadRun = New Thread(AddressOf Run)
            ThreadRun.Start()
        Catch
            noConnexion()
        End Try
    End Sub
    Private Sub setLabelJoueur1(ByVal val As String)
        des1Label.Text = val
    End Sub
    Private Sub setLabelJoueur2(ByVal val As String)
        des2Label.Text = val
    End Sub
    Private Sub setDes1(ByVal val As String)
        des1.Text = val
    End Sub
    Private Sub setDes2(ByVal val As String)
        des2.Text = val
    End Sub
    Private Sub activateButton()
        Button1.Enabled = True
    End Sub
    Private Sub desactivateButton()
        Button1.Enabled = False
    End Sub
    Private Sub Connexion()
        Try
            Envoi("connect", "id")
        Catch
            notificationLabel.Text = "Serveur injoignable ! "
        End Try
    End Sub
    Private Sub noConnexion()
        Button1.Enabled = False
        notificationLabel.Text = "Serveur injoignable ! "
    End Sub
    Private Sub notif(ByVal val As String)
        notificationLabel.Text = val
    End Sub
    Private Sub updateDesAdversaire(ByVal val As Integer)
        des2.Text = val
    End Sub
    Private Sub Run()

        Dim Message(255) As Byte

        While notConnected
            Message(255) = New Byte()
            Try
                SocketClient.Receive(Message)
                Dim SR = SocketClient.RemoteEndPoint.ToString
                traitementMessage(Message, SR)
            Catch ex As Exception
                Console.WriteLine("Erreur: " & ex.ToString)
            End Try
        End While
        ThreadEcoute = New Thread(AddressOf EcouteServeur)
        ThreadEcoute.Start()
        Console.WriteLine("Fin thread run")
    End Sub
    Private Sub Envoi(ByVal idMessage As String, ByVal message As String)

        Dim MessageEnvoi As String

        'concatenation de l'id et du message
        MessageEnvoi = idMessage + "-" + message
        Dim MesBytes(255) As Byte
        MesBytes(255) = New Byte()

        'conversion en tableau de Bytes
        MesBytes = System.Text.Encoding.ASCII.GetBytes(MessageEnvoi)

        'envoi du message au serveur
        Dim EnvoiMessage As Integer = SocketClient.Send(MesBytes)

    End Sub
    Private Sub traitementMessage(ByVal Message As Byte(), SR As String)

        'transformation du message recu en String
        Dim MessageRecu = System.Text.Encoding.ASCII.GetString(Message)

        'decoupe du message reçu
        Dim Identifiants(2) As String
        Identifiants = Split(MessageRecu, "-")

        Dim id As String = ""
        id = Identifiants(0)
        Dim val As String = ""
        val = Identifiants(1)

        'Switch permettant de traiter le message recu selon son id
        Select Case id
            Case "joueur"

                Invoke(Sub() notif("Vous êtes le joueur " + CStr(val.Chars(0))))
                Invoke(Sub() setDes1("0"))
                Invoke(Sub() setDes2("0"))
                Invoke(Sub() setLabelJoueur1("Votre dés : "))
                Invoke(Sub() setLabelJoueur2("Adversaire : "))

                notConnected = False
        End Select
    End Sub
    Private Sub EcouteServeur()
        Dim msg(255) As Byte
        Console.WriteLine("Lancement thread Ecoute")
        Try
            While True
                msg(255) = New Byte()
                SocketClient.Receive(msg)
                traitementServeur(msg)
            End While
        Catch
            Invoke(Sub() notif("Perte de connexion avec le serveur..."))
        End Try

    End Sub
    Private Sub traitementServeur(ByVal msg As Byte())

        'transformation du message recu en String
        Dim message = System.Text.Encoding.ASCII.GetString(msg)

        message = message.Replace(" ", "")
        Console.WriteLine("Message recu par le serveur => " + message)

        'decoupe du message reçu
        Dim Identifiants() As String
        Identifiants = Split(message, "-")

        Dim id As String = ""
        id = Identifiants(0)
        Dim val As String = ""
        val = Identifiants(1)

        'Switch permettant de traiter le message recu selon son id
        Select Case id
            Case "yourTurn"

                'on active le bouton de lancer
                Invoke(Sub() activateButton())
                'ancien emplacement adverse eteint
                Invoke(Sub() setCaseOff2(emplacementAdversaire))

                emplacementAdversaire = CInt(val)

                'nouvel emplacement adverse allumé
                Invoke(Sub() setCaseOn2(emplacementAdversaire))

            Case "notYourTurn"

                'on desactive le bouton de lancer 
                Invoke(Sub() desactivateButton())

            Case "lancerAdversaire"

                'mise a jour du label du des adverse
                Invoke(Sub() setDes2(val))

            Case "chute"

                'affichage d'une notification
                Invoke(Sub() notif("Aïe, vous chutez de 5 cases !"))
                System.Threading.Thread.Sleep(2000)

                'on eteint l'emplacement actuel
                setCaseOff1(emplacement)

                'on decremente l'emplacement
                emplacement -= 5

                'on allume le nouvel emplacement
                setCaseOn1(emplacement)

            Case "chuteAdverse"

                'affichage d'une notification
                Invoke(Sub() notif("Votre adversaire chute de 5 cases !"))
                System.Threading.Thread.Sleep(2000)

            Case "restart"

                'affichage d'une notification
                Invoke(Sub() notif("Catastrophe, vous repartez du début !!"))
                System.Threading.Thread.Sleep(2000)

                'on eteint l'emplacement actuel
                setCaseOff1(emplacement)

                'on remet l'emplacement à 0
                emplacement = 0

                'on allume le nouvel emplacement
                setCaseOn1(emplacement)

            Case "restartAdverse"

                'affichage d'une notification
                Invoke(Sub() notif("Le joueur adverse repart au début !!"))
                System.Threading.Thread.Sleep(2000)

                'on eteint l'emplacement actuel
                setCaseOff2(emplacementAdversaire)

                'on remet l'emplacement à 0
                emplacementAdversaire = 0

                'on allume le nouvel emplacement
                setCaseOn2(emplacementAdversaire)
        End Select
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        'génération d'un nombre aléatoire
        Dim val As Integer = 12
        Dim random As New Random()
        val = random.Next(1, 6)

        'envoi du resultat du lancer au serveur
        Envoi("valeur", CStr(val))

        'affichage du lancer effectué
        des1.Text = val

        'desactivation du bouton de lancer
        Button1.Enabled = False

        'sauvegarde de l'ancien emplacement + incrementation de l'emplacement actuel
        Dim oldEmplacement As Integer = emplacement
        emplacement += val

        'ancien emplacement eteint
        setCaseOff1(oldEmplacement)

        'nouvel emplacement allumé
        setCaseOn1(emplacement)

    End Sub
    Private Sub setCaseOn1(ByVal val As Integer)

        'on active la case correspondante (vert) au lancer de dé effectué
        casesJoueur1(val).BackColor = Color.Lime

    End Sub
    Private Sub setCaseOff1(ByVal val As Integer)

        'on desactive la case correspondante (gris ardoise) au lancer de dé effectué
        casesJoueur1(val).BackColor = Color.DarkSlateGray

    End Sub
    Private Sub setCaseOn2(ByVal val As Integer)

        'on active la case correspondante (rouge) au lancer de dé de l'adversaire
        casesJoueur2(val).BackColor = Color.Red

    End Sub
    Private Sub setCaseOff2(ByVal val As Integer)

        'on désactive la case correspondante (gris ardoise) au lancer de dé de l'adversaire
        casesJoueur2(val).BackColor = Color.DarkSlateGray

    End Sub
End Class
