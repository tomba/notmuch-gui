# libwebkitgtk-3.0-dev
# libxml-perl
# libxml-libxml-perl
# PKG_CONFIG_PATH=/opt/gtk-sharp-3/lib/pkgconfig make
#
GPARSER=gapi3-parser
GCODEGEN=gapi3-codegen

all: compile

mod-includes:
	cp -rf /usr/include/webkitgtk-3.0 .
	for file in webkitgtk-3.0/webkit/*.h; do			\
		sed -i -e 's,^WEBKIT_API ,/*WEBKIT_API*/\n,' $$file;	\
	done
	for file in webkitgtk-3.0/JavaScriptCore/*.h; do		\
		sed -i -e 's,^JS_EXPORT ,/*JS_EXPORT*/\n,' $$file;	\
	done

parser: mod-includes
	$(GPARSER) webkit.sources.xml

codegen: parser
	$(GCODEGEN) --outdir=generated --generate=webkit-api.raw `pkg-config --cflags gtk-sharp-3.0`

compile: codegen
	gmcs -unsafe -pkg:gtk-sharp-3.0 -target:library -out:webkit-sharp-3.0.dll generated/*/*.cs

clean:
	rm -rf generated webkit-api.raw *.dll webkitgtk-3.0

