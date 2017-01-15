Imports System.Net.Sockets
Imports System.Net
Imports System.Threading

Module Module1

    Dim port As String = "8081"
    Dim ip As String = "127.0.0.1"

    Dim client1 As String = "", client2 As String = ""
    Dim joueurs(2) As String
    Dim nbJoueurs As Integer = 0
    Dim emplacementJoueur1, emplacementJoueur2 As Integer

    Dim ThreadEcoute As Thread

    Dim socketReception As Socket
    Dim socketClient1, socketClient2 As Socket

    Dim notComplet As Boolean
    Sub Main()
        'emplcament des joueurs a la case 0
        emplacementJoueur1 = 0
        emplacementJoueur2 = 0

        'definition du socket du serveur 
        Dim SocketServeur As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        Dim MonEP As IPEndPoint = New IPEndPoint(IPAddress.Parse(ip), port)
        SocketServeur.Bind(MonEP)

        'mise du socket en écoute
        SocketServeur.Listen(1)

        'defintion d'un boolean permettant de savoir si la partie commence
        notComplet = True

        Console.WriteLine("Socket serveur initialisé...")
        Console.WriteLine("Aucun client connecté...")

        'tant que la partie n'est pas complete
        While notComplet
            Console.WriteLine("En attente...")
            socketReception = SocketServeur.Accept()
            Dim Message(255) As Byte
            Try
                socketReception.Receive(Message)
                'definition sur socket du joueur
                Dim SR = socketReception.RemoteEndPoint.ToString
                VerificationMessage(Message, SR)
            Catch ex As Exception
                Console.WriteLine("Erreur: " & ex.ToString)
            End Try
        End While
        Console.WriteLine("Lancement du jeu...")
        Console.WriteLine("En attente...")

    End Sub
    Private Sub VerificationMessage(ByVal Message As Byte(), SR As String)

        'transformation du message recu en String
        Dim MessageRecu = System.Text.Encoding.ASCII.GetString(Message)
        Console.WriteLine("-Message recu: " + MessageRecu)

        'decoupe du message reçu
        Dim Identifiants() As String
        Identifiants = Split(MessageRecu, "-")

        'si l'id du message est 'connect' et que seulement 0 ou 1 joueurs sont connectés
        If Identifiants(0) = "connect" And nbJoueurs < 2 Then
            If (nbJoueurs = 0) Then
                Console.WriteLine("Connexion de Client 1..")

                'definition du client 1
                client1 = SR

                'definition du socket client 1
                socketClient1 = socketReception

                'lancement d'un Thread d'ecoute sur le joueur 1
                ThreadEcoute = New Thread(AddressOf EcouteClient1)
                ThreadEcoute.Start()

                'Envoi d'un message pour signaler au joueur 1 qui il est
                Dim msg(64) As Byte
                msg = System.Text.Encoding.ASCII.GetBytes("joueur-1")
                socketClient1.Send(msg)
            Else
                Console.WriteLine("Connexion de Client 2..")

                'definition du client 2
                client2 = SR

                'definition du socket client 1
                socketClient2 = socketReception

                'lancement d'un Thread d'ecoute sur le joueur 2
                ThreadEcoute = New Thread(AddressOf EcouteClient2)
                ThreadEcoute.Start()

                'deux jours sont connectés
                notComplet = False

                'Envoi d'un message pour signaler au joueur 2 qui il est
                Dim msg(64) As Byte
                msg = System.Text.Encoding.ASCII.GetBytes("joueur-2")
                socketClient2.Send(msg)

                'Envoi d'un message au joueur 1 pour lui signaler que c'est son tour
                socketClient1.Send(System.Text.Encoding.ASCII.GetBytes("yourTurn-0"))

                'Envoi d'un message au joueur 2 pour lui signaler que ce n'est pas son tour
                socketClient2.Send(System.Text.Encoding.ASCII.GetBytes("notYourTurn-0"))

            End If
            'incrementation du nombre de joueurs
            nbJoueurs += 1

            'Si client 1 non-connecté
            If client1 <> "" Then
                Console.WriteLine("---Client 1 connecté => " + client1)
            Else
                Console.WriteLine("---Client 1 non-connecté.")
            End If

            'Si client 2 non-connecté
            If client2 <> "" Then
                Console.WriteLine("---Client 2 connecté => " + client2)
            Else
                Console.WriteLine("---Client 2 non-connecté.")
            End If
        End If
    End Sub
    Private Sub traitementJoueur1(ByVal msg As Byte())

        'transformation du message recu en String
        Dim message = System.Text.Encoding.ASCII.GetString(msg)
        message = message.Replace(" ", "")
        Console.WriteLine("-Message recu par joueur 1 => " + message)

        'decoupe du message reçu
        Dim Identifiants(2) As String
        Identifiants = Split(message, "-")

        Dim id As String = ""
        id = Identifiants(0)
        Dim val As String = ""
        val = Identifiants(1)

        'Switch permettant de traiter le message recu du joueur 1 selon son id
        Select Case id
            Case "valeur"

                'On incremente l'emplacement du joueur 1
                emplacementJoueur1 += CInt(val)

                'On signale au joueur 1 que ce n'est plus a lui de jouer
                Dim phrase4 As String
                phrase4 = "notYourTurn-0"
                For i = phrase4.Length To 29
                    phrase4 += " "
                Next
                Dim mess(64) As Byte
                mess = System.Text.Encoding.ASCII.GetBytes(phrase4)
                socketClient1.Send(mess)
                Console.WriteLine("--Message à joueur 1 -> notYourTurn-0")

                'Envoi du lancer de dé effectué par le joueur 1 au joueur 2
                Dim phrase3 As String
                phrase3 = "lancerAdversaire-" + val
                Console.WriteLine("--Message à joueur 2 -> " + phrase3)
                For i = phrase3.Length To 29
                    phrase3 += " "
                Next
                Dim messa(64) As Byte
                messa = System.Text.Encoding.ASCII.GetBytes(phrase3)
                socketClient2.Send(messa)

                'on verifie si l'emplacement correspond a une case speciale
                If emplacementJoueur1 = 6 Or emplacementJoueur1 = 16 Or emplacementJoueur1 = 25 Or emplacementJoueur1 = 36 Then

                    'le joueur avance de 5 cases
                    emplacementJoueur1 += 5

                    'on signale au joueur 1 qu'il fait un bond
                    Dim bond As String
                    bond = "bond-0"
                    For i = bond.Length To 29
                        bond += " "
                    Next
                    Dim bondMessage(64) As Byte
                    bondMessage = System.Text.Encoding.ASCII.GetBytes(bond)
                    socketClient1.Send(bondMessage)
                    Console.WriteLine("--Message à joueur 1 -> bond-0")
                    System.Threading.Thread.Sleep(1000)

                    'on signale au joueur 2 que le joueur 1 a bondi de 5 cases
                    Dim bondAdverse As String
                    bondAdverse = "bondAdverse-0"
                    For i = bondAdverse.Length To 29
                        bondAdverse += " "
                    Next
                    Dim bondMessage2(64) As Byte
                    bondMessage2 = System.Text.Encoding.ASCII.GetBytes(bondAdverse)
                    socketClient2.Send(bondMessage2)
                    Console.WriteLine("--Message à joueur 2 -> bondAdverse-0")

                ElseIf emplacementJoueur1 = 12 Or emplacementJoueur1 = 42 Or emplacementJoueur1 = 52 Then

                    'le joueur chute de 5 cases
                    emplacementJoueur1 -= 5
                    Dim chute As String
                    chute = "chute-0"
                    For i = chute.Length To 29
                        chute += " "
                    Next
                    Dim chuteMessage(64) As Byte
                    chuteMessage = System.Text.Encoding.ASCII.GetBytes(chute)
                    socketClient1.Send(chuteMessage)
                    Console.WriteLine("--Message à joueur 1 -> chute-0")
                    System.Threading.Thread.Sleep(3000)

                    'on signale au joueur 2 que le joueur 1 a chuté de 5 cases
                    Dim chuteAdverse As String
                    chuteAdverse = "chuteAdverse-0"
                    For i = chuteAdverse.Length To 29
                        chuteAdverse += " "
                    Next
                    Dim chuteMessage2(64) As Byte
                    chuteMessage2 = System.Text.Encoding.ASCII.GetBytes(chuteAdverse)
                    socketClient2.Send(chuteMessage2)
                    Console.WriteLine("--Message à joueur 2 -> chuteAdverse-0")

                ElseIf emplacementJoueur1 = 58 Then

                    'le joueur retombe à la case depart
                    emplacementJoueur1 = 0

                    'on signale au joueur 1 qu'il chute
                    Dim restart As String = ""
                    restart = "restart-0"
                    For i = restart.Length To 29
                        restart += " "
                    Next
                    Dim restartMessage(64) As Byte
                    restartMessage = System.Text.Encoding.ASCII.GetBytes(restart)
                    socketClient1.Send(restartMessage)
                    Console.WriteLine("--Message à joueur 1 -> restart-0")
                    System.Threading.Thread.Sleep(3000)

                    'on signale au joueur 2 que le joueur 1 retombe à la case départ
                    Dim restartAdverse As String = ""
                    restartAdverse = "restartAdverse-0"
                    For i = restartAdverse.Length To 29
                        restartAdverse += " "
                    Next
                    Dim restartAdverseMessage(64) As Byte
                    restartAdverseMessage = System.Text.Encoding.ASCII.GetBytes(restartAdverse)
                    socketClient2.Send(restartAdverseMessage)
                    Console.WriteLine("--Message à joueur 2 -> restartAdverse-0")

                ElseIf emplacementJoueur1 >= 60 Then

                    'on signale au joueur 1 qu'il gagné
                    Dim messageWin As String
                    messageWin = "youWin-0"
                    For i = messageWin.Length To 29
                        messageWin += " "
                    Next
                    Dim mesWin(64) As Byte
                    mesWin = System.Text.Encoding.ASCII.GetBytes(messageWin)
                    socketClient1.Send(mesWin)
                    Console.WriteLine("--Message à joueur 1 -> youWin-0")
                    System.Threading.Thread.Sleep(3000)

                    'on signale au joueur 2 qu'il a perdu
                    Dim messageLost As String
                    messageLost = "youLose-0"
                    For i = messageLost.Length To 29
                        messageLost += " "
                    Next
                    Dim mesLost(64) As Byte
                    mesLost = System.Text.Encoding.ASCII.GetBytes(messageLost)
                    socketClient2.Send(mesLost)
                    Console.WriteLine("--Message à joueur 2 -> youLose-0")

                    emplacementJoueur1 = 0
                    emplacementJoueur2 = 0

                End If

                System.Threading.Thread.Sleep(1000)
                'Au tour du joueur 2 de jeter le dé
                Dim phrase2 As String
                phrase2 = "yourTurn-" + CStr(emplacementJoueur1)
                For i = phrase2.Length To 29
                    phrase2 += " "
                Next
                Dim mes(64) As Byte
                mes = System.Text.Encoding.ASCII.GetBytes(phrase2)
                socketClient2.Send(mes)
                Console.WriteLine("--Message à joueur 2 -> yourTurn-0")

        End Select
    End Sub
    Private Sub traitementJoueur2(ByVal msg As Byte())

        'transformation du message recu en String
        Dim message = System.Text.Encoding.ASCII.GetString(msg)
        message = message.Replace(" ", "")

        Console.WriteLine("-Message recu par joueur 2 => " + message)

        'decoupe du message reçu
        Dim Identifiants(2) As String
        Identifiants = Split(message, "-")

        Dim id As String
        id = Identifiants(0)
        Dim val As String
        val = Identifiants(1)

        'Switch permettant de traiter le message recu du joueur 2 selon son id
        Select Case id
            Case "valeur"

                'On incremente l'emplacement du joueur 2
                emplacementJoueur2 += CInt(val)

                'On signale au joueur 2 que ce n'est plus a lui de jouer
                Dim phrase7 As String
                phrase7 = "notYourTurn-0"
                For i = phrase7.Length To 29
                    phrase7 += " "
                Next
                Dim mess(64) As Byte
                mess = System.Text.Encoding.ASCII.GetBytes(phrase7)
                socketClient2.Send(mess)
                Console.WriteLine("--Message à joueur 2 -> notYourTurn-0")

                'Envoi du lancer de dé effectué par le joueur 2 au joueur 1
                Dim phrase10 As String
                phrase10 = "lancerAdversaire-" + val
                Console.WriteLine("--Message à joueur 1 -> " + phrase10)
                For i = phrase10.Length To 29
                    phrase10 += " "
                Next
                Dim messa(64) As Byte
                messa = System.Text.Encoding.ASCII.GetBytes(phrase10)
                socketClient1.Send(messa)

                'on verifie si l'emplacement correspond a une case speciale
                If emplacementJoueur2 = 6 Or emplacementJoueur2 = 16 Or emplacementJoueur2 = 25 Or emplacementJoueur2 = 36 Then

                    'le joueur avance de 5 cases
                    emplacementJoueur2 += 5

                    'on signale au joueur 2 qu'il fait un bond
                    Dim bond As String
                    bond = "bond-0"
                    For i = bond.Length To 29
                        bond += " "
                    Next
                    Dim bondMessage(64) As Byte
                    bondMessage = System.Text.Encoding.ASCII.GetBytes(bond)
                    socketClient2.Send(bondMessage)
                    Console.WriteLine("--Message à joueur 2 -> bond-0")
                    System.Threading.Thread.Sleep(1000)

                    'on signale au joueur 1 que le joueur 2 a bondi de 5 cases
                    Dim bondAdverse As String
                    bondAdverse = "bondAdverse-0"
                    For i = bondAdverse.Length To 29
                        bondAdverse += " "
                    Next
                    Dim bondMessage2(64) As Byte
                    bondMessage2 = System.Text.Encoding.ASCII.GetBytes(bondAdverse)
                    socketClient1.Send(bondMessage2)
                    Console.WriteLine("--Message à joueur 1 -> bonAdverse-0")


                ElseIf emplacementJoueur2 = 12 Or emplacementJoueur2 = 42 Or emplacementJoueur2 = 52 Then

                    'le joueur chute de 5 cases
                    emplacementJoueur2 -= 5

                    'on signale au joueur 2 qu'il chute
                    Dim chute As String
                    chute = "chute-0"
                    For i = chute.Length To 29
                        chute += " "
                    Next
                    Dim chuteMessage(64) As Byte
                    chuteMessage = System.Text.Encoding.ASCII.GetBytes(chute)
                    socketClient2.Send(chuteMessage)
                    Console.WriteLine("--Message à joueur 2 -> chute-0")
                    System.Threading.Thread.Sleep(1000)

                    'on signale au joueur 1 que le joueur 2 a chuté de 5 cases
                    Dim chuteAdverse As String
                    chuteAdverse = "chuteAdverse-0"
                    For i = chuteAdverse.Length To 29
                        chuteAdverse += " "
                    Next
                    Dim chuteMessage2(64) As Byte
                    chuteMessage2 = System.Text.Encoding.ASCII.GetBytes(chuteAdverse)
                    socketClient1.Send(chuteMessage2)
                    Console.WriteLine("--Message à joueur 1 -> chuteAdverse-0")

                ElseIf emplacementJoueur2 = 58 Then

                    'le joueur retombe à la case depart
                    emplacementJoueur2 = 0

                    'on signale au joueur 2 qu'il chute
                    Dim restart As String
                    restart = "restart-0"
                    For i = restart.Length To 29
                        restart += " "
                    Next
                    Dim restartMessage(64) As Byte
                    restartMessage = System.Text.Encoding.ASCII.GetBytes(restart)
                    socketClient2.Send(restartMessage)
                    Console.WriteLine("--Message à joueur 2 -> restart-0")
                    System.Threading.Thread.Sleep(3000)

                    'on signale au joueur 1 que le joueur 2 retombe à la case départ
                    Dim restartAdverse As String
                    restartAdverse = "restartAdverse-0"
                    For i = restartAdverse.Length To 29
                        restartAdverse += " "
                    Next
                    Dim restartAdverseMessage(64) As Byte
                    restartAdverseMessage = System.Text.Encoding.ASCII.GetBytes(restartAdverse)
                    socketClient1.Send(restartAdverseMessage)
                    Console.WriteLine("--Message à joueur 1 -> restartAdverse-0")

                ElseIf emplacementJoueur2 >= 60 Then

                    'on signale au joueur 1 qu'il a perdu
                    Dim messageLost As String
                    messageLost = "youLose-0"
                    For i = messageLost.Length To 29
                        messageLost += " "
                    Next
                    Dim mesLost(64) As Byte
                    mesLost = System.Text.Encoding.ASCII.GetBytes(messageLost)
                    socketClient1.Send(mesLost)
                    Console.WriteLine("--Message à joueur 1 -> youLose-0")

                    System.Threading.Thread.Sleep(3000)

                    'on signale au joueur 2 qu'il gagné
                    Dim messageWin As String
                    messageWin = "youWin-0"
                    For i = messageWin.Length To 29
                        messageWin += " "
                    Next
                    Dim mesWin(64) As Byte
                    mesWin = System.Text.Encoding.ASCII.GetBytes(messageWin)
                    socketClient2.Send(mesWin)
                    Console.WriteLine("--Message à joueur 2 -> youWin-0")

                    emplacementJoueur1 = 0
                    emplacementJoueur2 = 0

                End If

                System.Threading.Thread.Sleep(1000)
                'Au tour du joueur 1 de jeter le dé
                Dim phrase9 As String
                phrase9 = "yourTurn-" + CStr(emplacementJoueur2)
                For i = phrase9.Length To 29
                    phrase9 += " "
                Next
                Dim mes(64) As Byte
                mes = System.Text.Encoding.ASCII.GetBytes(phrase9)
                socketClient1.Send(mes)
                Console.WriteLine("--Message à joueur 1 -> yourTurn-0")

        End Select
    End Sub
    Private Sub EcouteClient1()
        Dim msg(255) As Byte
        Try
            While True
                msg(255) = New Byte()
                socketClient1.Receive(msg)
                traitementJoueur1(msg)
            End While
        Catch ex As Exception
            Console.WriteLine("Perte du client 1...")
            Console.WriteLine(ex)
        End Try
    End Sub
    Private Sub EcouteClient2()
        Dim msg(255) As Byte
        Try
            While True
                msg(255) = New Byte()
                socketClient2.Receive(msg)
                traitementJoueur2(msg)
            End While
        Catch
            Console.WriteLine("Perte du client 2...")
        End Try
    End Sub
End Module