# Resumen para resolver ejercicios — U2 a U4  
Límites · Continuidad · TVI · Derivadas · TVM · Análisis de funciones

Idea general: en cada ejercicio identificá **qué te piden** y seguí el **orden fijo** de esa sección. No inventes pasos: el libro trabaja casi siempre con la misma receta.

---

# 1. Límites

## Qué te están pidiendo
Que digas a qué número (o ±∞) se acerca \(f(x)\) cuando \(x\) se acerca a un valor \(a\), o a \(\pm\infty\).  
El valor de \(f(a)\) **no importa** para el límite (salvo que uses continuidad después).

## Cómo resolverlos (orden)

### Paso 1 — Sustitución directa
Reemplazá \(x\) por \(a\).
- Sale un número → ese es el límite. Listo.
- Sale \(k/0\) con \(k\neq 0\) → el límite es infinito o no existe como número: mirá **laterales**.
- Sale \(0/0\) o \(\infty/\infty\) → **forma indeterminada**: hay que reescribir.

### Paso 2 — Si es 0/0, reescribí
Buscá una función igual a \(f\) en todos lados excepto en \(a\), y ahí sí aplicá leyes.

| Aspecto de la expresión | Técnica |
|-------------------------|---------|
| Polinomios / racionales | Factorizar y **cancelar** el factor \((x-a)\) |
| Diferencia con raíz | Multiplicar num y den por el **conjugado** |
| Fracción dentro de fracción | Simplificar a **una sola** fracción |
| Trigonométricas básicas | Identidades + límites conocidos (\(\frac{\sin\theta}{\theta}\to 1\), etc.) o emparedado |

Después de simplificar, **volvé a sustituir**.

### Paso 3 — Límites laterales
Calculá \(\lim_{x\to a^-}\) y \(\lim_{x\to a^+}\).
- Si son el **mismo** número \(L\) → \(\lim_{x\to a}f(x)=L\).
- Si son distintos → el límite **bilateral no existe**.
- Si alguno es \(\pm\infty\) → no existe como número real (podés decir “diverge a ±∞” si el enunciado lo admite).

### Paso 4 — Límites al infinito
Para racionales: compará grados (o dividí todo por la potencia más alta de \(x\)).
- Grado den > grado num → límite \(0\) (asíntota horizontal \(y=0\)).
- Grados iguales → límite = cociente de coeficientes líderes.
- Grado num > grado den → \(\pm\infty\) (y si la diferencia de grados es 1, puede haber asíntota oblicua).

### Paso 5 — L’Hôpital (cuando aplica)
Solo si quedó **0/0** o **∞/∞** y las funciones son derivables cerca del punto:
\[
\lim\frac{f}{g}=\lim\frac{f'}{g'}
\]
(si este último existe). Si sigue indeterminado, repetí. Si **no** es 0/0 ni ∞/∞, **no** uses L’Hôpital: reescribí primero.

### Emparedado (teorema del sandwich)
Si \(g(x)\le f(x)\le h(x)\) cerca de \(a\) y \(\lim g=\lim h=L\), entonces \(\lim f=L\).  
Se usa cuando no podés simplificar algebraicamente (típico con \(\sin\) o \(\cos\) acotados).

### Checklist de un ejercicio de límite
1. ¿Qué tipo de límite? (\(x\to a\), laterales, \(x\to\pm\infty\))  
2. ¿Sustitución directa?  
3. ¿Indeterminada? ¿Cuál técnica de reescritura?  
4. ¿Laterales necesarios?  
5. ¿La respuesta es número, ±∞ o “no existe”?

---

# 2. Continuidad

## Definición operativa (para ejercicios)
\(f\) es **continua en \(a\)** si se cumplen las **tres** a la vez:
1. \(f(a)\) está definida  
2. Existe \(\lim_{x\to a}f(x)\) (laterales iguales y finitos)  
3. \(\lim_{x\to a}f(x)=f(a)\)

Si falla cualquiera → **discontinua en \(a\)**.

## Cómo clasificar la discontinuidad
| Tipo | Cómo la reconocés |
|------|-------------------|
| **Removible** | El límite existe, pero \(f(a)\) no está o no coincide. Se “arregla” redefiniendo \(f(a)=\lim\). |
| **De salto** | Laterales finitos y **distintos**. |
| **Infinita** | Algún lateral es \(\pm\infty\) (suele haber asíntota vertical). |

## Continuidad en un intervalo
- Abierto \((a,b)\): continua en **cada** punto interior.  
- Cerrado \([a,b]\): continua en el interior + continuidad **por la derecha** en \(a\) y **por la izquierda** en \(b\).

## Ejercicios típicos: “hallá \(k\) para que sea continua”
1. En los trozos donde ya es “buena” (polinomio, etc.) no hay problema.  
2. En el punto de empalme \(a\): imponé que existan laterales y que \(\lim=f(a)\).  
3. Eso te da una ecuación (o sistema) para \(k\).

## Operaciones
Suma, producto, cociente (den ≠ 0) y composición de continuas → continuas donde tengan sentido.

---

# 3. Teorema del valor intermedio (TVI) y bisección

## Enunciado (lo que tenés que verificar)
Si \(f\) es **continua** en \([a,b]\) y \(z\) está **entre** \(f(a)\) y \(f(b)\), entonces existe al menos un \(c\in[a,b]\) con \(f(c)=z\).

**Importante:** exige continuidad en el cerrado. Si hay un “agujero” o salto en el medio, **no** aplica.

## Para qué sirve en ejercicios
Casi siempre: **demostrar que hay una raíz** (un cero) o que una ecuación tiene solución.

### Receta para “demostrá que tiene un cero en …”
1. Verificá que \(f\) es continua en el intervalo (o en todo \(\mathbb{R}\)).  
2. Evaluá \(f\) en dos extremos (o en dos puntos que elijas).  
3. Si \(f(a)\) y \(f(b)\) tienen **signos opuestos**, entonces \(0\) está entre \(f(a)\) y \(f(b)\).  
4. Por TVI existe \(c\) con \(f(c)=0\).

El TVI **no** te da el valor de \(c\); solo garantiza que existe.  
Tampoco garantiza que **no** haya otras raíces fuera, ni que haya una sola.

## Método de bisección (idea práctica del TVI)
Cuando te piden un intervalo chico que contenga la raíz:
1. Partí de \([a,b]\) con \(f(a)\cdot f(b)<0\).  
2. Tomá el punto medio \(m=\frac{a+b}{2}\).  
3. Mirás el signo de \(f(m)\):  
   - Si \(f(a)\cdot f(m)<0\) → la raíz está en \([a,m]\).  
   - Si \(f(m)\cdot f(b)<0\) → está en \([m,b]\).  
4. Repetí hasta que el intervalo tenga la longitud pedida (ej. \(0{,}01\)).

Es el TVI aplicado una y otra vez, achicando el intervalo a la mitad.

---

# 4. Derivadas

## Qué es (para no confundirte)
\(f'(a)\) es la pendiente de la tangente / tasa de cambio instantánea:
\[
f'(a)=\lim_{h\to 0}\frac{f(a+h)-f(a)}{h}
\]
Si ese límite no existe → **no derivable** en \(a\).

**Relación con continuidad:** derivable ⇒ continua.  
Al revés es falso (esquina, valor absoluto en 0, salto, etc.).

## Cómo derivar en la práctica
1. Identificá la forma: suma, producto, cociente, composición, implícita.  
2. Aplicá la regla correspondiente.  
3. Si hay composición → **cadena** (de afuera hacia adentro).  
4. Simplificá solo lo necesario para el siguiente paso del problema.

### Reglas (memorizar)
| Regla | Fórmula |
|-------|---------|
| Constante | \((c)'=0\) |
| Potencia | \((x^n)'=nx^{n-1}\) |
| Múltiplo | \((cf)'=cf'\) |
| Suma/resta | \((f\pm g)'=f'\pm g'\) |
| Producto | \((fg)'=f'g+fg'\) |
| Cociente | \(\bigl(\frac{f}{g}\bigr)'=\frac{f'g-fg'}{g^2}\) |
| Cadena | \(\bigl(f(g(x))\bigr)'=f'(g(x))\cdot g'(x)\) |

### Básicas
\((\sin x)'=\cos x\),\quad \((\cos x)'=-\sin x\),\quad \((\tan x)'=\sec^2 x\)  
\((e^x)'=e^x\),\quad \((\ln x)'=\frac{1}{x}\),\quad \((\log_b x)'=\frac{1}{x\ln b}\)  
\((\arcsin x)'=\frac{1}{\sqrt{1-x^2}}\),\quad \((\arctan x)'=\frac{1}{1+x^2}\)

## Diferenciación implícita
Cuando la ecuación mezcla \(x\) e \(y\) y no conviene despejar \(y\):
1. Derivá **ambos lados** respecto de \(x\).  
2. Cada vez que deriva una \(y\), aparece \(y'\) (cadena).  
3. Agrupá términos con \(y'\) y despejá \(y'\).  
4. Si piden la tangente en un punto, reemplazá \(x,y\) (y \(y'\) si hace falta) en la fórmula de la recta.

## Derivada de la inversa
Si \(y=f^{-1}(x)\), \(b=f(a)\) y \(f'(a)\neq 0\):
\[
(f^{-1})'(b)=\frac{1}{f'(a)}
\]

## Tasas relacionadas
1. Nombrá variables y dibujá si hace falta.  
2. Escribí qué tasa **dan** y cuál **piden** (todas respecto del tiempo, en general).  
3. Ecuación que relaciona las variables (Pitágoras, área, volumen, semejantes…).  
4. Diferenciá ambos lados respecto de \(t\) (**después** de tener la ecuación).  
5. Recién ahí sustituí los valores numéricos y despejá la tasa desconocida.  
**Error típico:** sustituir un valor variable antes de derivar (esa variable deja de “moverse”).

## Aproximaciones lineales
\[
L(x)=f(a)+f'(a)(x-a)
\]
Sirve para estimar \(f\) cerca de \(a\). El diferencial es \(dy=f'(x)\,dx\).

---

# 5. Teorema de Rolle y teorema del valor medio (TVM)

## Teorema de Rolle
Si \(f\) es continua en \([a,b]\), derivable en \((a,b)\) y \(f(a)=f(b)\),  
entonces existe \(c\in(a,b)\) con \(f'(c)=0\).

**Uso:** demostrar que hay un punto donde la tangente es horizontal; o que entre dos raíces de \(f\) hay una raíz de \(f'\).

## Teorema del valor medio (TVM)
Si \(f\) es continua en \([a,b]\) y derivable en \((a,b)\), entonces existe \(c\in(a,b)\) tal que
\[
f'(c)=\frac{f(b)-f(a)}{b-a}
\]
Interpretación: en algún punto la pendiente de la **tangente** iguala la de la **secante** entre los extremos.

### Cómo resolver ejercicios de TVM
1. **Verificar hipótesis:** continuidad en el cerrado + derivabilidad en el abierto.  
   Si falla alguna → el teorema **no se aplica** (y a veces el \(c\) ni existe).  
2. Si se aplica y te piden el/los \(c\):  
   - Calculá \(\dfrac{f(b)-f(a)}{b-a}\).  
   - Resolvé \(f'(x)=\) ese número, con \(x\in(a,b)\).  
3. Si te piden explicar por qué **no** hay tal \(c\): mostrá que falta continuidad o derivabilidad (esquina, discontinua, etc.).

### Corolarios útiles (salen del TVM)
- Si \(f'=0\) en todo un intervalo → \(f\) es **constante** ahí.  
- Si \(f'=g'\) → \(f=g+C\).  
- Si \(f'>0\) → \(f\) **creciente**; si \(f'<0\) → **decreciente**.  
  (Esto justifica las tablas de signos de la primera derivada.)

---

# 6. Análisis completo de funciones (graficar / estudiar \(f\))

Hacé **siempre** este orden. Es la receta del libro para “dibujá / analizá la función”.

## A. Dominio
Excluís: denominador cero, raíces de índice par de negativos, logaritmos de no positivos, etc.  
Anotá el dominio en intervalos.

## B. Intersecciones y datos básicos
- Corte con eje \(y\): \(f(0)\) (si 0 está en el dominio).  
- Cortes con eje \(x\): resolvé \(f(x)=0\) (o argumentá con TVI si no se puede exacto).  
- Par / impar (opcional): ahorra trabajo al graficar.  
- Periodicidad (si es trig).

## C. Asíntotas y comportamiento al infinito
**Verticales:** candidatos donde “se rompe” el dominio (den = 0).  
Para cada candidato, mirá laterales: si \(\to\pm\infty\), hay AV.

**Horizontales:** \(\lim_{x\to\pm\infty}f(x)=L\) → \(y=L\).

**Oblicuas:** en racionales, si grado(num) = grado(den) + 1, dividí polinomios:  
\(f(x)=mx+b+\frac{r(x)}{q(x)}\) y \(y=mx+b\) es la asíntota.

## D. Primera derivada — crecimiento y extremos locales
1. Calculá \(f'(x)\).  
2. **Puntos críticos:** \(f'(x)=0\) o \(f'\) no existe, **dentro del dominio de \(f\)**.  
3. Armá una **tabla de signos** de \(f'\) en los subintervalos.  
4. Conclusiones:  
   - \(f'>0\) → creciente  
   - \(f'<0\) → decreciente  
5. **Prueba de la primera derivada** en cada crítico:  
   - signo de \(f'\) pasa de − a + → **mínimo local**  
   - de + a − → **máximo local**  
   - no cambia → no hay extremo local (punto de inflexión horizontal a veces, pero no extremo)

## E. Extremos absolutos en un intervalo cerrado \([a,b]\)
(Teorema de extremos: si \(f\) es continua en el cerrado, los absolutos existen.)

1. Evaluá \(f(a)\) y \(f(b)\).  
2. Evaluá \(f\) en **todos** los críticos interiores.  
3. Compará esos valores: el mayor es el máximo absoluto; el menor, el mínimo absoluto.  
Indicá **dónde** se alcanzan (el \(x\)), no solo el valor.

## F. Segunda derivada — concavidad e inflexión
1. Calculá \(f''(x)\).  
2. Puntos donde \(f''=0\) o no existe.  
3. Tabla de signos de \(f''\):  
   - \(f''>0\) → cóncava **hacia arriba**  
   - \(f''<0\) → cóncava **hacia abajo**  
4. Si cambia la concavidad en un punto donde \(f\) es continua → **punto de inflexión**.  
   (Que \(f''=0\) **no alcanza**: tiene que **cambiar** el signo.)

**Prueba de la segunda derivada** (alternativa en un crítico con \(f'(c)=0\)):
- \(f''(c)>0\) → mínimo local  
- \(f''(c)<0\) → máximo local  
- \(f''(c)=0\) → **inconclusa** → volvé a la prueba de la primera

## G. Cierre del análisis
Resumí en un cuadro:
- dominio  
- asíntotas  
- intervalos de crecimiento / decrecimiento  
- extremos locales y absolutos (si piden)  
- concavidad e inflexiones  
- bosquejo del gráfico coherente con todo eso

## Optimización (problemas de máximo/mínimo aplicados)
1. Variables + figura.  
2. ¿Qué cantidad maximizar/minimizar?  
3. Fórmula de esa cantidad.  
4. Con restricciones, dejala en **una sola variable**.  
5. Dominio **físico** (longitudes > 0, etc.).  
6. Críticos + extremos del dominio → elegí el que cumple el enunciado.

---

# 7. Mapa rápido: “me dan el ejercicio, ¿por dónde empiezo?”

| Si el enunciado dice… | Empezá por… |
|-----------------------|-------------|
| Calculá / evaluá el límite | Sustitución → reescritura → laterales / ∞ |
| ¿Es continua? / clasificá discontinuidad | Las 3 condiciones; mirá laterales |
| Demostrá que hay raíz / solución | Continuidad + signos opuestos + **TVI** (bisección si piden intervalo chico) |
| Derivá / hallá \(f'\) | Reglas + cadena; implícita si hace falta |
| Hallá \(c\) del TVM / Rolle | Hipótesis → \(\frac{f(b)-f(a)}{b-a}\) → \(f'(c)=\) eso |
| Extremos en \([a,b]\) | Extremos del intervalo + críticos |
| Intervalos ↑↓ / máx-mín locales | \(f'\), tabla de signos, prueba 1ª |
| Concavidad / inflexión | \(f''\), cambio de signo |
| Graficá / análisis completo | Orden A→G de la sección 6 |
| Maximizá / minimizá (palabra) | Optimización: 1 variable + dominio físico |
| 0/0 o ∞/∞ “difícil” | Reescribir; si sigue, L’Hôpital |

---

# 8. Errores que más restan

- Cancelar factores sin chequear que era 0/0 (o cancelar y olvidar el agujero).  
- Decir que el límite existe cuando los laterales no coinciden.  
- Aplicar TVI o TVM sin verificar continuidad / derivabilidad.  
- Usar L’Hôpital fuera de 0/0 o ∞/∞.  
- Sustituir valores en tasas relacionadas **antes** de derivar.  
- Confundir punto crítico con extremo (no todo crítico es máx/mín).  
- Decir inflexión solo porque \(f''=0\) (falta el cambio de concavidad).  
- En extremos absolutos, olvidarse de evaluar los **extremos del intervalo**.
