# Privacidade

A extensão Android via Wi-Fi usa o ADB do Android SDK instalado no computador. Não implementa telemetria própria, cadastro ou envio de dados a serviços de análise.

Para cumprir sua função, lê anúncios locais de descoberta e a lista de dispositivos do ADB. Nas operações solicitadas pelo usuário, o ADB comunica-se com o dispositivo Android informado ou descoberto. O ADB pode manter chaves e informações de pareamento em seus diretórios padrão; a extensão não substitui esse mecanismo.

Os códigos temporários são transmitidos ao processo ADB pela entrada padrão. A extensão não os adiciona aos argumentos de processo e remove o segredo das mensagens retornadas pelo cliente implementado no projeto.

O comportamento do próprio Visual Studio, do Android SDK, do sistema operacional e de outras extensões segue as políticas desses produtos.
