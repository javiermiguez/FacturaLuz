# FacturaLuz

FacturaLuz é unha pequena aplicación que fixen para un caso moi específico:
precisaba calcular o custo da electricidade dunha parte concreta da instalación eléctrica da casa.
Para iso instalei un dispositivo medidor na liña que quería controlar e desenvolvín este programa para procesar os datos.

É un proxecto persoal e unha boa escusa para repasar **C#** e **LINQ** e aprender **WinUI 3**.
Comparto o código aquí por se lle pode servir a alguén como referencia ou exemplo.

---

##  Que fai a aplicación

- Le os datos dun **Shelly EM** (a través da súa API HTTP) para o período que indique o usuario.
- Obtén os prezos horarios da electricidade usando a **API de ESIOS** para ese mesmo intervalo.
- Cruza ambos conxuntos de datos e, a partir dos valores definidos na configuración,
  calcula o custo total e mostra o desglose igual que o dunha factura eléctrica real.

---

## Tecnoloxías empregadas

- **C#**
- **.NET 8**
- **WinUI 3**
- **LINQ**
- **ESIOS API**
- **Shelly EM HTTP API**

---

## Estado do proxecto

Trátase dun proxecto persoal en continua aprendizaxe. Non é unha aplicación deseñada para o público en xeral,
senón unha solución concreta a unha necesidade doméstica.

---

## Licenza

Este proxecto publícase baixo a licenza **MIT**.
Podes usar, copiar, modificar e distribuír o código libremente, sempre baixo a túa responsabilidade.

---

---

# FacturaLuz (English)

FacturaLuz is a small application I built for a very specific use case:
I needed to calculate the electricity cost of a particular branch of my home’s electrical installation.
To do that, I installed a power meter at the desired point and developed this program to process the data.

It's a personal project and a good excuse to revisit **C#** and **LINQ**, and learn **WinUI 3**.
I'm sharing the code here in case it might be useful as a reference or example for others.

---

## What the application does

- Reads data from a **Shelly EM** device (via HTTP API) for a user-specified date range.
- Fetches hourly electricity prices from the **ESIOS API** for the same period.
- Combines both data sources and, using values defined in the configuration,
  calculates the total cost and generates a breakdown similar to an actual utility bill.

---

## Tech stack

- **C#**
- **.NET 8**
- **WinUI 3**
- **LINQ**
- **ESIOS API**
- **Shelly EM HTTP API**

---

## Project status

This is a personal learning project. It is not intended as a general-purpose app.
It solves a very specific household need.

---

## License

This project is released under the **MIT License**.
You can use, copy, modify, and distribute it freely, at your own responsibility.