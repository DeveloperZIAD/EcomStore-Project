# ÇáãÊÛíÑÇÊ
COMPOSE=docker-compose

# ÇáÃãÑ ÇáÇİÊÑÇÖí ÚäÏ ßÊÇÈÉ 'make' İŞØ
all: build up

# ÈäÇÁ ÇáÕæÑ (Images) ãä ÌÏíÏ
build:
	$(COMPOSE) build

# ÊÔÛíá ÇáÍÇæíÇÊ İí ÇáÎáİíÉ
up:
	$(COMPOSE) up -d

# ÊÔÛíá ÇáÍÇæíÇÊ ãÚ ÑÄíÉ ÇáÓÌáÇÊ (Logs) ãÈÇÔÑÉ
run:
	$(COMPOSE) up --build

# ÅíŞÇİ ÇáÍÇæíÇÊ
down:
	$(COMPOSE) down

# ÅíŞÇİ ÇáÍÇæíÇÊ æãÓÍ ÇáÈíÇäÇÊ (Volumes) - ÍĞÑ ÚäÏ ÇÓÊÎÏÇãå
clean:
	$(COMPOSE) down -v

# ÑÄíÉ ÍÇáÉ ÇáÍÇæíÇÊ
status:
	$(COMPOSE) ps

# ÑÄíÉ ÓÌáÇÊ ÇáÈÇß ÅäÏ İŞØ áãÊÇÈÚÉ ÇáÜ Migrations
logs-api:
	$(COMPOSE) logs -f backend