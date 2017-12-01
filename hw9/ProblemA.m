% load values from csv file, downloaded from internet
values = csvread('bitcoin_value.csv',1,1)

% compute the log returns
log_returns = log(values(2:end)./values(1:end-1))

% fit the generalized hyperbolic with log returns
params = ghfit(log_returns,100,1)

% x axis
x = linspace(-0.5,0.5,100)

% generalized hyperbolic with the fitted parameters
y = ghpdf(x, params(1:3), params(4),params(5))

% plot PDF and histogram
figure

subplot(2,1,1);
plot(x,y)
hold on
histogram(log_returns, 200, 'Normalization', 'pdf')
grid on
xlim([-0.2 0.2])
% Add title and axis labels
xlabel('Log returns')
ylabel('histogram and fitted PDF')
title('Generalized Hyperbolic Distribution')

% plot PDF and histogram in log space
subplot(2,1,2);
semilogy(x,y)
hold on
histogram(log_returns, 200, 'Normalization', 'pdf')
grid on
xlim([-0.2 0.2])
% Add title and axis labels
xlabel('Log returns')
ylabel('log histogram and fitted PDF')
title('Generalized Hyperbolic Distribution')