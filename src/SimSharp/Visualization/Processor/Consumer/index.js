const express = require('express');
const app = express();
const port = 3000;
const WebSocket = require('ws');
const wss = new WebSocket.Server({ port: 8080 });
const q = 'simSharpQueue';
const open = require('amqplib').connect('amqp://localhost');

wss.on('connection', function connection(ws) {
    open.then(function (conn) {
        return conn.createChannel();
    }).then(function (ch) {
        return ch.assertQueue(q).then(function (ok) {
            return ch.consume(q, function (msg) {
                if (msg !== null) {
                    ws.send(msg.content.toString());
                    ch.ack(msg);
                }
            });
        });
    }).catch(console.warn);
});

app.use(express.static('public'))

app.get('/', (req, res) => {
    res.sendFile('./Player.html', { root: __dirname });
});

app.listen(port, () => console.log(`listening on port ${port}!`))