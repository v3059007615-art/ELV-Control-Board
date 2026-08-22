//http://zhoulifa.bokee.com/6065720.html 2016-04-19 18:37:28 Modified slightly
//Compile this program with the following commands£º
//gcc -Wall broadc-udpclient.c -o client
//Run the program with the following commands£º
//./client 255.255.255.255 60000
//Send messages to all hosts in the network¡£

#include <stdio.h>
#include <string.h>
#include <sys/types.h>
#include <netinet/in.h>
#include <sys/socket.h>
#include <errno.h>
#include <stdlib.h>
#include <arpa/inet.h>


/*********************************************************************
*filename: broadc-udpclient.c
*purpose: Description of basic programming steps£¬It's a demonstration.UDPProgramming client programming step for programming
*tidied by: zhoulifa(zhoulifa@163.com) Zhou Lifa(http://zhoulifa.bokee.com)
LinuxLovers LinuxKnowledge disseminaters SOHOGroup Developer I'm good at it.CLanguages
*date time:2007-01-24 21:30:00
*Note: Anyone can copy the codes and apply them.£¬Including, of course, your commercial use.
* But please follow.GPL
*Thanks to: Google.com
*Hope:I hope more and more people will contribute.£¬To develop science and technology
* Technology is moving faster on the shoulders of giants.£¡Thank you for your contribution.£¡
*********************************************************************/
int main(int argc, char **argv)
{
struct sockaddr_in s_addr;
int sock;
int addr_len;
int len;
//char buff[64];
//Search controller
//char buff[]={0x17,0x94,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00};
//Open remote[Broadcast mode]
char buff[]={0x17,0x40,0x00,0x00,0xFF,0xFF,0xFF,0xFF,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00};

int yes;

/* Create socket */
if ((sock = socket(AF_INET, SOCK_DGRAM, 0)) == -1) {
perror("socket");
exit(errno);
} else
printf("create socket.\n\r");

/* Set up communication to broadcast£¬A message from this program£¬All hosts on the network are available. */
yes = 1;
setsockopt(sock, SOL_SOCKET, SO_BROADCAST, &yes, sizeof(yes));
/* The only change is that. */

/* Set each other 's address and port information */
s_addr.sin_family = AF_INET;
if (argv[2])
s_addr.sin_port = htons(atoi(argv[2]));
else
s_addr.sin_port = htons(60000);
if (argv[1])
s_addr.sin_addr.s_addr = inet_addr(argv[1]);
else {
printf("The message must have a recipient.£¡\n");
exit(0);
}

/* SendUDPMessage */
addr_len = sizeof(s_addr);
//strcpy(buff, "hello i'm here");
//len = sendto(sock, buff, strlen(buff), 0,
len = sendto(sock, buff, 64, 0,
(struct sockaddr *) &s_addr, addr_len);
if (len < 0) {
printf("\n\rsend error.\n\r");
return 3;
}

printf("send success.\n\r");
return 0;
}