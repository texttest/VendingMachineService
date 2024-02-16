#!/usr/bin/env python

import logging, os, sys, socket, webbrowser, time
from subprocess import Popen, PIPE
import capturemock
import dbtext


def find_available_port():
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        s.bind(("", 0))
        s.listen(1)
        return s.getsockname()[1]


def wait_for_file(fn):
    for _ in range(600):
        if os.path.isfile(fn) and os.path.getsize(fn) > 0:
            time.sleep(1)  # allow time for flush
            return
        else:
            time.sleep(0.1)
    print("Timed out waiting for action after 1 minute!", file=sys.stderr)


def wait_for_message(proc, ready_text):
    ready_bytes = ready_text.encode()
    for _ in range(100):
        msg_bytes = proc.stdout.readline()
        logging.info(f"SUT: {msg_bytes.decode('utf-8')}")
        if ready_bytes in msg_bytes:
            break


if __name__ == "__main__":
    logging.getLogger().addHandler(logging.StreamHandler(sys.stdout))
    logging.getLogger().setLevel(logging.INFO)

    testdbname = "ttdb_" + str(os.getpid())  # some temporary name not to clash with other tests
    with dbtext.LocalMongo_DBText(data_dirname=testdbname) as db:
        db.create()
        if not db.setup_succeeded():
            logging.error("Failed to start mongodb")
            sys.exit(1)

    port = find_available_port()
    url = f"http://127.0.0.1:{port}"
    db_conn_string = "mongodb://localhost:" + str(db.port)

    my_env = os.environ.copy()
    my_env["VendingMachineDatabase:ConnectionString"] = db_conn_string
    my_env["PORT"] = str(port)
    my_env["Kestrel:EndPoints:Http:Url"] = url
    my_env["ASPNETCORE_URLS"] = url
    my_env["URLS"] = url

    record = capturemock.texttest_is_recording()
    if record:
        capturemock_env = capturemock.start_server_from_texttest(url)
        my_env.update(capturemock_env)

    texttest_home = os.path.dirname(os.environ.get("TEXTTEST_ROOT"))
    command = ["dotnet", "run", "--project", f"{texttest_home}/VendingMachineService.csproj"]

    logging.info(f"starting VendingMachine on url {url} attaching to db {db_conn_string}")

    process = Popen(command, stdout=PIPE, env=my_env)
    wait_for_message(process, "Content root path")
    logging.info(f"VendingMachine is started")
    try:
        if record:
            swagger_path = "/swagger/index.html"
            logging.debug(f"Will record swagger interations on page {swagger_path}")
            webbrowser.open_new(url + swagger_path)
            wait_for_file(os.getenv("TEXTTEST_CAPTUREMOCK_RECORD"))
        else:
            capturemock.replay_for_server(serverAddress=url)
    finally:
        logging.info("stopping Vending Machine")
        process.terminate()
        capturemock.terminate()

    if "TEXTTEST_DB_SETUP" in os.environ:
        db.dump_data_directory("mongodata")
    db.dump_changes("machine")
