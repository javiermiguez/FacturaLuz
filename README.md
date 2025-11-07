FacturaLuz é unha pequena aplicación que fixen para un caso de uso moi concreto: Necesitaba saber o custe do consumo dunha parte da miña instalación eléctrica, así que instalei un dispositivo medidor no punto desexado e desenvolvín este programa. Isto pareceume unha boa excusa para repasar C#, Linq e aprender WinUI 3. Publico o código por se a alguén lle resulta útil como referencia ou exemplo. Calquera pode usalo, modificalo e adaptalo libremente baixo a súa responsabilidade.

Que fai a aplicación:
- Le os datos do Shelly EM (via HTTP API) para un período indicado polo usuario.
- Obtén os prezos horarios da electricidade usando a API de ESIOS para ese mesmo período.
- A partires deses datos e dos valores gardados na configuración, calcula o custe total e desglósao como nunha factura da compañía eléctrica.