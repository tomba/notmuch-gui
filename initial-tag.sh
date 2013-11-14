#!/bin/bash

set -e

mails=(`notmuch config get user.primary_email`)
mails+=(`notmuch config get user.other_email`)

to=(${mails[@]})
to=(${to[@]/#/to:})
to=${to[@]}
to=${to// / OR }
to="($to)"

from=(${mails[@]})
from=(${from[@]/#/from:})
from=${from[@]}
from=${from// / OR }
from="($from)"

#set -x

new="tag:new AND "
new=

notmuch tag --batch <<-end
	-inbox +to-me -- $new $to

	-inbox +from-me -- $new $from

	-inbox +linux-kernel -- $new to:linux-kernel@vger.kernel.org
	-inbox +linux-omap -- $new to:linux-omap@vger.kernel.org
	-inbox +linux-fbdev -- $new to:linux-fbdev@vger.kernel.org
	-inbox +linux-arm -- $new to:linux-arm-kernel@lists.infradead.org

	-inbox +users-kernel-org -- $new to:users@linux.kernel.org

	-new -- tag:new
end

