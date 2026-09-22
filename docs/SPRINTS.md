# Backlog y Planificación de Sprints

Proyecto: **Sistema de Simulación de Créditos**
Marco de trabajo: adaptación de Scrum a micro-sprints (3 sprints × 5 días = 15 días)

---

## Product Backlog — Historias de Usuario

| ID | Historia de usuario | Criterios de aceptación | Sprint |
|---|---|---|---|
| HU-01 | Como visitante, quiero **registrarme** con correo y contraseña para tener una cuenta en el simulador. | El correo es único; la contraseña se guarda con hash (nunca en texto plano); se rechazan correos mal formados. | 1 |
| HU-02 | Como usuario registrado, quiero **iniciar sesión** para obtener un token que me dé acceso al simulador. | Credenciales válidas devuelven un JWT firmado con expiración; credenciales inválidas devuelven 401 sin revelar cuál dato falló. | 1 |
| HU-03 | Como usuario, quiero una **pantalla de login** clara para acceder sin fricción. | Formulario con validación en vivo, mensajes de error legibles y persistencia del token en el navegador. | 1 |
| HU-04 | Como usuario autenticado, quiero **consultar los tipos de crédito** disponibles y su tasa para elegir con información. | El endpoint devuelve los tres tipos con su tasa anual leída de la base de datos. | 2 |
| HU-05 | Como usuario autenticado, quiero **simular un crédito** indicando monto, plazo y tipo para conocer mis cuotas. | La tasa se deriva del tipo elegido; se devuelven las tablas francesa y alemana en un único JSON. | 2 |
| HU-06 | Como responsable del sistema, quiero que **los endpoints de crédito exijan JWT** para que nadie simule sin autenticarse. | Una petición sin token o con token vencido devuelve 401. | 2 |
| HU-07 | Como usuario, quiero un **formulario de simulación** cómodo para lanzar el cálculo. | Validación de monto y plazo en rangos permitidos, con mensajes en español. | 3 |
| HU-08 | Como usuario, quiero **ver las dos tablas de amortización** lado a lado para compararlas. | Tablas con cuota, interés, capital y saldo por período, más totales, en formato de moneda. | 3 |
| HU-09 | Como usuario, quiero una **interfaz atractiva y comprensible** para usar el simulador con confianza. | Diseño responsive, estados de carga y error, y coherencia visual en todas las pantallas. | 3 |

---

## Sprint 1 — Cimientos y Autenticación (Días 1 a 5)

**Objetivo del sprint:** que el sistema permita registro y acceso seguro con JWT almacenado.

Historias: HU-01, HU-02, HU-03

Tareas técnicas:
- [x] Configurar PostgreSQL y crear `authdb`.
- [x] Crear la solución .NET 10 y el proyecto `Auth.Api`.
- [x] Modelar la entidad `Usuario` y generar la migración inicial con EF Core.
- [x] Implementar hash de contraseñas con BCrypt.
- [x] Implementar `POST /api/auth/register` y `POST /api/auth/login` con emisión de JWT.
- [x] Configurar CORS para la SPA.
- [x] Crear el frontend con Vite + React + TypeScript + Tailwind.
- [x] Implementar la vista de Login y el almacenamiento del token.

**Incremento entregable:** un usuario puede registrarse, iniciar sesión y conservar su sesión.

### Pruebas de aceptación ejecutadas al cierre

| # | Caso | Resultado esperado | Obtenido |
|---|---|---|---|
| 1 | `GET /health` | Servicio activo | ✅ 200 |
| 2 | Registro de usuario nuevo | 201 con JWT y perfil | ✅ token de 435 caracteres |
| 3 | Registro con correo repetido | 409 y mensaje explicativo | ✅ 409 |
| 4 | Login con contraseña incorrecta | 401 sin revelar qué dato falló | ✅ 401 |
| 5 | Login correcto | 200 con JWT vigente | ✅ 200 |
| 6 | `GET /api/auth/me` con token | Perfil del portador | ✅ 200 |
| 7 | `GET /api/auth/me` sin token | 401 | ✅ 401 |
| 8 | `GET /api/auth/me` con firma alterada | 401 | ✅ 401 |
| 9 | Registro con datos inválidos | 400 con mensajes en español | ✅ 400 |
| 10 | Preflight CORS desde `localhost:5173` | Origen permitido | ✅ cabeceras emitidas |
| 11 | Preflight CORS desde origen ajeno | Sin cabeceras `Access-Control` | ✅ rechazado |
| 12 | Contraseña almacenada en la base | Hash BCrypt, nunca texto plano | ✅ `$2a$11$…` |

---

## Sprint 2 — Lógica de Simulación (Días 6 a 10)

**Objetivo del sprint:** API capaz de recibir datos y retornar tablas matemáticamente exactas.

Historias: HU-04, HU-05, HU-06

Tareas técnicas:
- [x] Crear el proyecto `Credit.Api` y su contexto sobre `creditdb`.
- [x] Sembrar el catálogo de tipos de crédito con sus tasas (15.50 / 8.50 / 22.00).
- [x] Implementar la regla: tipo de crédito → tasa de interés.
- [x] Programar el algoritmo de amortización francés.
- [x] Programar el algoritmo de amortización alemán.
- [x] Validar la exactitud con pruebas unitarias (xUnit).
- [x] Proteger los endpoints exigiendo el JWT emitido por Auth API.
- [x] Persistir el historial de simulaciones del usuario.

**Incremento entregable:** endpoints de simulación funcionales y verificados.

### Endpoints entregados (Credit API, puerto 5090)

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/creditos/tipos` | Catálogo de tipos de crédito con su tasa vigente |
| `POST` | `/api/creditos/simular` | Recibe tipo, monto y plazo; devuelve tablas francesa y alemana |
| `GET` | `/api/creditos/historial` | Últimas 50 simulaciones del usuario autenticado |

Todos exigen `Authorization: Bearer <JWT>` emitido por la Auth API.

### Pruebas unitarias: 42 en verde

| Grupo | Qué verifica |
|---|---|
| Método francés | Cuota de referencia de manual (10 000 al 12 % a 12 meses = 888.49), cuota idéntica en todas las filas, interés decreciente y capital creciente, crédito sin interés |
| Método alemán | Caso calculado a mano (12 000 al 12 %: 1 120 → 1 010, interés total 780), capital constante, cuota decreciente, menor interés total que el francés |
| Integridad (6 escenarios × 2 métodos) | Suma de capital = monto exacto, saldo final 0, cada fila cuadra, saldos encadenados, cuota fija (francés) y capital fijo (alemán) al centavo |
| Regla de negocio | Conversión a tasa mensual de las 3 tasas, mayor tasa = mayor costo, rechazo de montos, plazos y tasas inválidos |

### Defecto detectado y corregido durante el sprint

La primera versión del motor redondeaba la cuota francesa a centavos y luego
iteraba sobre el saldo redondeado. La fracción de centavo que se dejaba de pagar
cada mes se **capitalizaba con (1+i)ⁿ**: en un crédito de 100 000 al 22 % a 480
meses, la última "cuota fija" salía en **2 693.27 frente a 1 833.63**, 859.64 de
desviación. Las pruebas de sumas y saldos pasaban igual, porque la última fila
absorbía la diferencia.

Lo detectó la prueba que verifica la propiedad que *define* al método francés, y
no solo sus totales. Corrección: cada fila se ancla al saldo teórico exacto,
calculado con precisión decimal completa, y el error de redondeo nunca supera un
centavo ni se acumula. El caso quedó como prueba de regresión.

### Pruebas de aceptación ejecutadas al cierre

| # | Caso | Resultado esperado | Obtenido |
|---|---|---|---|
| 1 | Los 3 endpoints sin token | 401 | ✅ 401 en los 3 |
| 2 | Token con emisor válido pero firmado con otra clave | 401 | ✅ 401 |
| 3 | Token real de la Auth API usado en la Credit API | Aceptado | ✅ 200 |
| 4 | Catálogo de tipos | 3 tipos leídos de `creditdb` | ✅ 15.50 / 8.50 / 22.00 |
| 5 | Simular con cada tipo | La tasa aplicada es la del tipo | ✅ en los 3 tipos |
| 6 | Enviar `tasaAnual: 0.01` en el cuerpo | Se ignora; manda el tipo | ✅ aplica 22.00 |
| 7 | Tipo inexistente | 400 con mensaje | ✅ |
| 8 | Monto fuera de rango (50 y 5 000 000) | 400 con mensaje | ✅ |
| 9 | Plazo fuera de rango (0 y 600) | 400 con mensaje | ✅ |
| 10 | Historial del usuario | Sus simulaciones, más reciente primero | ✅ |
| 11 | Usuario nuevo consulta historial | Vacío: no ve las de otros | ✅ `[]` |
| 12 | Simulación de un usuario | No altera el historial de otro | ✅ |
| 13 | Claves foráneas de `creditdb` | Ninguna apunta a `authdb` | ✅ solo `simulaciones → tipos_credito` |

---

## Sprint 3 — Interfaz Atractiva y Cierre (Días 11 a 15)

**Objetivo del sprint:** plataforma finalizada, interactiva y lista para revisión del usuario final.

Historias: HU-07, HU-08, HU-09

Tareas técnicas:
- [x] Diseñar el formulario de simulación en React.
- [x] Renderizar ambas tablas de amortización devueltas por el API.
- [x] Mostrar el resumen comparativo (cuota inicial, total de intereses, total pagado).
- [x] Aplicar validaciones de errores de usuario y estados de carga.
- [x] Pulido visual y responsive.
- [x] Documentar la puesta en marcha en el README.

**Incremento entregable:** producto completo listo para demostración.

### Pantallas y componentes entregados

| Componente | Responsabilidad |
|---|---|
| `FormularioSimulacion` | Tarjetas de tipo de crédito con su tasa, monto, plazo con atajos y validación en vivo |
| `ResumenSimulacion` | Cuota fija, cuota alemana inicial y final, intereses de cada método y cuál conviene |
| `TablaAmortizacion` | Tabla con cuota, interés, capital y saldo por período, encabezado fijo y totales |
| `HistorialSimulaciones` | Simulaciones previas del usuario, con estado vacío propio |

Decisiones de interfaz:

- **Encabezado fijo y desplazamiento propio en las tablas.** Un crédito a 480
  meses genera 480 filas; sin esto la página se vuelve inmanejable y se pierde
  de vista qué significa cada columna.
- **Importes con `Intl.NumberFormat('es-EC')` y cifras tabulares**, para que las
  columnas de dinero queden alineadas y sean comparables de un vistazo.
- **La tabla del método más económico se resalta**, de modo que la comparación
  se entienda sin leer los totales.
- **Validación en el cliente con los mismos rangos que el API.** No la
  reemplaza: el servidor sigue validando, pero el usuario recibe el error al
  instante en lugar de esperar un viaje de red.

### Pruebas de aceptación ejecutadas al cierre

| # | Caso | Resultado esperado | Obtenido |
|---|---|---|---|
| 1 | Compilación del frontend (`tsc` + `vite build`) | Sin errores de tipos | ✅ 40 módulos |
| 2 | Linter (`oxlint`) | Sin advertencias | ✅ limpio |
| 3 | Login desde la SPA | Token recibido | ✅ 435 caracteres |
| 4 | Carga del catálogo | 3 tarjetas con sus tasas | ✅ 15.50 / 8.50 / 22.00 |
| 5 | Simulación de 15 000 a 24 meses | Dos tablas de 24 filas | ✅ 48 filas |
| 6 | Tasa aplicada según el tipo | La del tipo elegido | ✅ 15.50 % |
| 7 | Comparativo | Método más económico y diferencia | ✅ alemán, $119.00 |
| 8 | Historial tras simular | Se actualiza solo | ✅ 5 registros |

### Pulido realizado sobre el código del Sprint 1

El linter señaló tres problemas heredados que se corrigieron:

1. `AuthContext` exportaba a la vez el contexto y un componente, lo que rompía
   el refresco en caliente de Vite. El contexto se movió a `contextoAuth.ts`.
2. El estado de carga se actualizaba de forma síncrona dentro de un efecto,
   provocando un render en cascada. Ahora se inicializa con el valor correcto.
3. Los accesos a `localStorage` no estaban protegidos: en modo privado lanzan
   excepción. Se envolvieron en `try/catch`, conservando la sesión en memoria.

---

## Iteración de rediseño — Retroalimentación del cliente

**Origen:** tras revisar el Sprint 3, el cliente pidió rediseñar la interfaz
tomando como referencia el simulador de créditos del Banco Pichincha. Es el valor
ágil *colaboración con el cliente sobre negociación contractual* aplicado: el
producto funcionaba, pero el cliente redefinió qué significa "atractivo".

### Estudio del referente

El simulador del banco es una aplicación JavaScript que no puede leerse
descargando la página, así que su flujo se reconstruyó a partir de guías
especializadas. La identidad visual, en cambio, se extrajo directamente del HTML
y el CSS de su sitio.

| Aspecto | Hallazgo en el referente | Decisión |
|---|---|---|
| Flujo | Pasos secuenciales: producto, monto, plazo, frecuencia, seguro, calcular | **Adoptado:** formulario en 4 pasos numerados |
| Resultado | Cuota e ingreso mínimo requerido como cifras principales | **Adoptado:** panel de resultado fijo a la derecha |
| Frecuencia de pago | Mensual a semestral, o al vencimiento | **Adoptado** (requirió cambios en el backend) |
| Seguro de desgravamen | Opcional, se elige antes de calcular | **Adoptado** (requirió cambios en el backend) |
| Ingreso mínimo | Se muestra junto a la cuota | **Adoptado** (requirió cambios en el backend) |
| Cierre de página | Aviso legal y preguntas frecuentes en acordeón | **Adoptado** |
| Tipo de persona y subproducto | Natural o jurídica; siete subproductos | **No adoptado:** fuera del alcance acordado |
| Colores (`#FFDD00`, `#0F265C`) y tipografía Prelo | Identidad de marca del banco | **No adoptado:** se conserva la paleta propia |

Se tomó la estructura y no la identidad de marca por decisión del cliente. Evita
además que un repositorio público se confunda con un producto de una institución
financiera real. Como sustituto libre de Prelo, que es una tipografía comercial,
se usó *Source Sans 3*.

### Cambios en el backend

- **Frecuencia de pago** (`FrecuenciaPago`): cambia los meses por cuota, la tasa
  periódica y el número de cuotas. El plazo debe ser múltiplo de la frecuencia.
- **Seguro de desgravamen**: prima sobre el saldo al inicio de cada período. La
  tasa vive en `tipos_credito`, por producto, igual que la tasa de interés.
- **Ingreso mínimo requerido**: cuota más alta, llevada a su equivalente mensual,
  sobre una relación cuota/ingreso del 40 %, redondeada hacia arriba.

### Pruebas: 66 en verde

Las 42 anteriores siguen pasando sin modificación: las firmas originales del
motor se conservaron como atajos (mensual y sin seguro). Se agregaron 24, todas
con valores calculados a mano. Por ejemplo: 12 000 al 12 % trimestral a 12 meses
en alemán da cuotas de 3 360 a 3 090 e interés total de 900.

### Defectos evitados durante la iteración

1. **La migración habría roto el historial.** EF Core generó la columna
   `FrecuenciaPago` con valor vacío para las simulaciones existentes, que no es
   una frecuencia válida: leer el historial habría fallado con error 500. Se
   corrigió para asignar `Mensual`, que es lo que eran, y se reconstruyó su
   ingreso mínimo con la misma regla del motor.
2. **Los errores exponían detalles internos.** Un valor inválido en el JSON
   devolvía, en inglés, el nombre interno del tipo (`Credit.Api.Models.FrecuenciaPago`).
   Ahora esos errores se responden en español y sin detalles de implementación.
3. **Tasas duplicadas en el login.** El panel de acceso tenía las tasas escritas
   en el código, desconectadas de `creditdb`: habrían quedado desactualizadas
   al primer cambio. Se reemplazaron por los beneficios del producto.

---

## Iteración de reestructuración — Alineación con la guía de la asignatura

**Origen:** la guía de la asignatura define una estructura de referencia para los
proyectos de microservicios. El proyecto funcionaba, pero organizaba el código
con otra convención, así que se reorganizó para ajustarse a ella.

### Correspondencia con la estructura de referencia

| Estructura de la guía | Antes | Ahora |
|---|---|---|
| `backend/<Nombre>Service/` | `Auth.Api/`, `Credit.Api/` | `AuthService/`, `CreditService/` |
| `Dominio/` | `Models/` | `Dominio/` |
| `Aplicacion/` | `Dtos/` + `Services/` | `Aplicacion/Dtos/` + `Aplicacion/Servicios/` |
| `Estructura/` | `Data/` | `Estructura/` |
| `Presentacion/` | `Controllers/` | `Presentacion/` |
| `Migrations/` | `Data/Migraciones/` | `Migrations/` |
| `ApiGateway/` | no existía | creado con YARP |
| `database/database.sql` | no existía | generado desde las migraciones |
| `frontend/<nombre>-web/` | app en la raíz de `frontend/` | `frontend/creditos-web/` |
| `iniciar.bat` | no existía | creado |

### Decisiones tomadas

- **Se conservan los nombres Auth y Credit.** La guía nombra el servicio de
  identidad `IndentityService`, pero la especificación oficial de este proyecto
  llama a los servicios **Auth API** y **Credit API**. Se adoptó la convención
  `<Nombre>Service` sin contradecir el documento propio.
- **Se mantiene el proyecto de pruebas**, que la estructura de referencia no
  contempla. Eliminar 66 pruebas para cuadrar con un diagrama habría cambiado
  un diagrama por una garantía real de que los cálculos son correctos.
- **`ErroresEnEspanol` pasó a `Presentacion/`** y no a `Aplicacion/`: da forma a
  las respuestas HTTP, que es responsabilidad de la capa de presentación.
- **El gateway no valida el JWT.** Solo enruta; cada servicio valida su token.
  Validar también en el gateway obligaría a mantener la misma clave de firma en
  tres lugares, con el riesgo de que se desincronicen.

### Efecto en el frontend

La SPA ahora habla con un único origen, el gateway (`http://localhost:5000`), en
lugar de conocer el puerto de cada microservicio. Si un servicio cambia de
puerto, basta con editar `ApiGateway/appsettings.json`; antes había que tocar y
recompilar la interfaz.

### Verificación

- 66 pruebas unitarias en verde después de mover todos los archivos y reescribir
  los espacios de nombres.
- Compilación de la solución y del frontend sin advertencias; linter limpio.
- Enrutamiento comprobado extremo a extremo por el puerto 5000: login, perfil,
  catálogo y simulación. Una petición sin token sigue devolviendo 401 y una ruta
  no mapeada, 404, de modo que el gateway no debilitó la seguridad.

---

## Registro de evidencias

Se completa al cierre de cada sprint.

| Sprint | Fecha de cierre | Incremento demostrado | Adaptaciones realizadas |
|---|---|---|---|
| 1 | 16/09/2026 | Registro, login y sesión persistente con JWT verificado contra la base `authdb`. 12 pruebas de aceptación en verde. | Se conservó **PostgreSQL 17**, ya instalado en el equipo, en lugar de instalar la 18: dos servidores en el mismo puerto habrían costado tiempo de configuración sin aportar valor al producto. Ejemplo directo del principio *responder ante el cambio sobre seguir un plan*. |
| 2 | 18/09/2026 | Credit API con regla tipo → tasa, tablas francesa y alemana exactas al centavo, historial por usuario y endpoints protegidos con el JWT de la Auth API. 42 pruebas unitarias y 13 de aceptación en verde. | Se corrigió un defecto de redondeo acumulativo en el método francés, detectado por las propias pruebas del sprint. Se añadió una prueba de regresión y se documentó el caso. Se unificó la versión de EF Core (10.0.12) para eliminar un conflicto de dependencias. |
| 3 | 20/09/2026 | Plataforma completa: formulario de simulación, tablas francesa y alemana con totales, resumen comparativo, historial e interfaz responsive. 8 pruebas de aceptación en verde. | Se añadió la vista de historial, que no figuraba en el plan original: la Credit API ya persistía las simulaciones y sin pantalla esa funcionalidad quedaba invisible. Se corrigieron tres defectos heredados del Sprint 1 detectados por el linter. |
| Rediseño | 21/09/2026 | Interfaz reestructurada según el simulador de referencia, con frecuencia de pago, seguro de desgravamen e ingreso mínimo requerido. 66 pruebas unitarias en verde. | El cliente pidió replicar un referente comercial; se adoptó su estructura y se descartó su identidad de marca. Se evitaron dos defectos antes de que llegaran a producción: una migración que habría roto el historial y mensajes de error que exponían nombres internos. |
| Reestructuración | 22/09/2026 | Proyecto reorganizado según la estructura de referencia de la asignatura: capas Dominio, Aplicacion, Estructura, Presentacion y Migrations en cada servicio, más ApiGateway, `database/database.sql`, `frontend/creditos-web/` e `iniciar.bat`. 66 pruebas en verde tras el cambio. | Se conservaron los nombres Auth y Credit para no contradecir la especificación propia del proyecto, y se mantuvo el proyecto de pruebas, que la estructura de referencia no contempla. |
